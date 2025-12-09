using System;
using System.Collections.Concurrent;
using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Loggers;
using WebApi.Settings;

namespace WebApi.Helpers;

/// <summary>
/// Helper class for Cloudinary operations.
/// </summary>
public class CloudinaryHelper
{
    private readonly Cloudinary _cloudinary;
    private readonly MyBlogLogger _logger;
    private readonly CloudinarySettings _settings;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private const int MaxConcurrentUploads = 5;

    public CloudinaryHelper(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;
        _logger = new MyBlogLogger("cloudinary.log");

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException($"File {file?.FileName} is empty.");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException($"File {file.FileName} exceeds 10MB.");

        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new InvalidOperationException($"File type is not allowed: {file.ContentType}");
    }

    private async Task<T> Retry<T>(Func<Task<T>> func, string taskName = "", int retries = 3)
    {
        Exception lastException = null!;
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                lastException = ex;

                await _logger.LogWarning($"Retry {taskName}", $"Attempt {attempt}: {ex.Message}");

                if (attempt == retries)
                    break;

                // waiting 200ms, 400ms, 800ms after each attempt
                await Task.Delay(200 * (int)Math.Pow(2, attempt - 1));
            }
        }
        throw lastException ?? new Exception("Retry failed");
    }

    /// <summary>
    /// Internal method to handle the actual upload from a safe MemoryStream
    /// </summary>
    private async Task<ImageDto> UploadFromStream(string fileName, MemoryStream stream)
    {
        await _logger.LogInfo($"UploadFromStream {fileName}");
        var uploadParams = new ImageUploadParams { File = new FileDescription(fileName, stream) };

        var uploadResult = await Retry(
            async () =>
            {
                // Reset stream position for every attempt (Crucial for retries)
                stream.Position = 0;
                return await _cloudinary.UploadAsync(uploadParams);
            },
            $"Upload {fileName}"
        );

        if (uploadResult.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Cloudinary upload failed: {uploadResult.StatusCode}");

        return new ImageDto
        {
            PublicId = uploadResult.PublicId,
            Link = uploadResult.SecureUrl.ToString(),
        };
    }

    /// <summary>
    /// Upload a single IFormFile.
    /// </summary>
    public async Task<ImageDto> Upload(IFormFile file)
    {
        await _logger.LogInfo($"Upload {file.FileName}");
        ValidateFile(file);

        // Copy to memory immediately to isolate from Request stream
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        try
        {
            return await UploadFromStream(file.FileName, memoryStream);
        }
        catch (Exception ex)
        {
            await _logger.LogError($"Upload {file.FileName}", error: ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Uploads multiple images.
    /// </summary>
    //
    // Workflow Explanation:
    // Suppose we have 5 files: A, B, C, D, E
    // A -> Upload success -> Mark as finished
    // B -> Upload success -> Mark as finished
    // C -> Failed -> Retry (Attempt 1) -> Go back the begin of the file (stream.Position = 0) every attempt
    // C -> Failed -> Retry (Attempt 2) -> Go back the begin of the file
    // C -> Success -> Mark as finished
    // D -> Upload success -> Mark as finished
    // E -> Upload failed -> Retry (1, 2, 3) -> All failed -> Rollback A, B, C, D -> throw exception
    // and so on...
    // SemaphoreSlim to limit concurrency
    // If we have 10 pics, and MaxConcurrentUploads = 5
    // First 5 pics start uploading concurrently (A, B, C, D, E)
    // When one finishes (A removed), the next one starts (F added), until all are done

    // Note: Must read IFormFiles sequentially due to non-thread-safe Request stream,
    public async Task<List<ImageDto>> UploadImages(IList<IFormFile> files)
    {
        await _logger.LogInfo($"UploadImages (Count: {files?.Count ?? 0})");

        if (files == null || files.Count == 0)
            return new List<ImageDto>();

        // 1. PHASE ONE: SEQUENTIAL BUFFERING
        // We must read IFormFiles sequentially because the underlying Request stream is not thread-safe.
        var preparedFiles = new List<(string FileName, MemoryStream Stream)>();

        try
        {
            foreach (var file in files)
            {
                ValidateFile(file);
                var ms = new MemoryStream();
                await file.CopyToAsync(ms); // Sequential Read
                ms.Position = 0;
                preparedFiles.Add((file.FileName, ms));
            }
        }
        catch (Exception ex)
        {
            await _logger.LogError("UploadImages - Buffering Failed", ex.Message);
            // Clean up any streams opened so far
            foreach (var item in preparedFiles)
                item.Stream.Dispose();
            throw;
        }

        // 2. PHASE TWO: PARALLEL UPLOAD
        // Now that data is in safe MemoryStreams, we can upload concurrently.
        var uploadedImages = new ConcurrentBag<ImageDto>();
        var errors = new ConcurrentBag<Exception>();
        using var semaphore = new SemaphoreSlim(MaxConcurrentUploads);

        var tasks = preparedFiles.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                var img = await UploadFromStream(item.FileName, item.Stream);
                uploadedImages.Add(img);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                await _logger.LogError($"Upload Failed: {item.FileName}", ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        foreach (var item in preparedFiles)
        {
            item.Stream.Dispose();
        }

        // 3. ROLLBACK ON FAILURE
        if (!errors.IsEmpty)
        {
            var firstError = errors.First();
            await _logger.LogError("UploadImages Failed - Rolling back", firstError.Message);

            var idsToDelete = uploadedImages.Select(img => img.PublicId).ToList();
            if (idsToDelete.Any())
            {
                await DeleteImages(idsToDelete);
            }

            throw new Exception(
                $"Upload batch failed. First error: {firstError.Message}",
                firstError
            );
        }

        return uploadedImages.ToList();
    }

    public async Task<bool> Delete(string publicId)
    {
        await _logger.LogInfo($"Delete {publicId}");
        try
        {
            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            await _logger.LogError($"Delete {publicId}", error: ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteImages(List<string> publicIds)
    {
        await _logger.LogInfo($"DeleteImages (Count: {publicIds?.Count ?? 0})");
        if (publicIds == null || publicIds.Count == 0)
            return true;

        using var semaphore = new SemaphoreSlim(MaxConcurrentUploads);
        var success = true;

        var tasks = publicIds.Select(async id =>
        {
            await semaphore.WaitAsync();
            try
            {
                if (!await Delete(id))
                    success = false;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return success;
    }
}
