using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Dtos;
using WebApi.Settings;

namespace WebApi.Helpers;

public class CloudinaryHelper
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryHelper>? _logger;
    private readonly CloudinarySettings _settings;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };

    public CloudinaryHelper(
        IOptions<CloudinarySettings> options,
        ILogger<CloudinaryHelper>? logger = null
    )
    {
        _settings = options.Value;
        _logger = logger;

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    /// <summary>
    ///  Validate file before upload
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when the file is invalid</exception>
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException($"File {file?.FileName} is empty.");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException($"File {file.FileName} exceeds 10MB.");

        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new InvalidOperationException($"File type is not allowed: {file.ContentType}");
    }

    /// <summary>
    /// Retry logic for transient errors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="retries"></param>
    /// <returns></returns>
    private async Task<T> Retry<T>(Func<Task<T>> func, int retries = 2)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                attempt++;
                _logger?.LogWarning(ex, "Attempt {Attempt} failed", attempt);

                if (attempt > retries)
                {
                    _logger?.LogError(ex, "Retry failed after {Attempt} attempts", attempt);
                    throw;
                }

                await Task.Delay(200); // tránh spam
            }
        }
    }

    /// <summary>
    /// Delete image by public ID
    /// </summary>
    /// <param name="publicId">The public ID of the image to delete</param>
    /// <returns>True if the image was successfully deleted; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> Delete(string publicId)
    {
        try
        {
            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete image: {PublicId}", publicId);
            return false;
        }
    }

    /// <summary>
    /// Delete multiple images by their public IDs
    /// </summary>
    /// <param name="publicIds">List of public IDs to delete</param>
    /// <returns>True if all images were successfully deleted; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> DeleteImages(List<string> publicIds)
    {
        if (publicIds == null || publicIds.Count == 0)
            return true;

        bool success = true;

        foreach (var id in publicIds)
        {
            if (!await Delete(id))
                success = false;
        }

        return success;
    }

    /// <summary>
    /// Upload a single image file to Cloudinary
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>The uploaded image details</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file is invalid</exception>
    /// <exception cref="Exception">Thrown when the upload fails</exception>
    public async Task<ImageDto> Upload(IFormFile file)
    {
        ValidateFile(file);

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
        };

        var uploadResult = await Retry(() => _cloudinary.UploadAsync(uploadParams));

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Cloudinary upload failed.");

        return new ImageDto
        {
            PublicId = uploadResult.PublicId,
            Link = uploadResult.SecureUrl.ToString(),
        };
    }

    /// <summary>
    /// Upload multiple image files to Cloudinary with rollback on failure
    /// </summary>
    /// <param name="files">The list of image files to upload</param>
    /// <returns>List of successfully uploaded image details</returns>
    /// <exception cref="Exception">Thrown when any upload fails</exception>
    public async Task<List<ImageDto>> UploadImages(IList<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return new List<ImageDto>();

        var uploaded = new List<ImageDto>();

        try
        {
            foreach (var file in files)
            {
                var img = await Upload(file);
                uploaded.Add(img);
            }

            return uploaded;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Upload failed. Rolling back...");

            foreach (var img in uploaded)
                await Delete(img.PublicId);

            throw;
        }
    }
}
