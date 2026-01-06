using Imagekit.Sdk;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Loggers;
using WebApi.Settings;

namespace WebApi.Helpers;

public class UploadHelper
{
    private readonly MyBlogLogger _logger;
    private readonly UploadSettings _uploadSettings;

    private readonly ImagekitClient imagekit;

    public UploadHelper(MyBlogLogger logger, IOptions<UploadSettings> options)
    {
        _logger = logger;
        _uploadSettings = options.Value;
        imagekit = new ImagekitClient(
            _uploadSettings.PublicKey,
            _uploadSettings.PrivateKey,
            _uploadSettings.UrlEndpoint
        );
    }

    public async Task<ImageDto> UploadImageAsync(IFormFile file)
    {
        await _logger.LogInfo("UploadHelper", $"Uploading image {file.FileName}");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        byte[] fileBytes = ms.ToArray();

        FileCreateRequest request = new()
        {
            file = fileBytes,
            fileName = file.FileName,
            folder = "uploads",
        };

        Result resp = await imagekit.UploadAsync(request);

        if (resp.HttpStatusCode == 200)
        {
            await _logger.LogInfo("UploadHelper", $"Upload successfully {file.FileName}");
        }
        else
        {
            await _logger.LogError("UploadHelper", $"Upload failed {file.FileName}");
        }

        return resp.HttpStatusCode == 200
            ? new ImageDto { PublicId = resp.fileId, Link = resp.url }
            : null!;
    }

    public async Task DeleteImageAsync(List<string> fileIds)
    {
        foreach (var fileId in fileIds)
        {
            var result = await imagekit.DeleteFileAsync(fileId);
        }
    }

    // public async Task<ImageDto> UploadImageAsync(IFormFile file)
    // {
    //     if (file == null || file.Length == 0)
    //     {
    //         throw new ArgumentException("File is null or empty", nameof(file));
    //     }

    //     // 1. Đọc file thành byte[]
    //     byte[] bytes;
    //     using (var ms = new MemoryStream())
    //     {
    //         await file.CopyToAsync(ms);
    //         bytes = ms.ToArray();
    //     }

    //     // 2. Convert sang Base64
    //     var base64Image = Convert.ToBase64String(bytes);

    //     // 3. Gọi API ibb
    //     var apiKey = "b9a4d9322875f65c9bf9fbedfc2a310a";
    //     var url = $"https://api.imgbb.com/1/upload?key={apiKey}";

    //     using var client = new HttpClient();
    //     var form = new MultipartFormDataContent();
    //     form.Add(new StringContent(base64Image), "image");

    //     var response = await client.PostAsync(url, form);
    //     // using var doc = System.Text.Json.JsonDocument.Parse(
    //     //     response.Content.ReadAsStringAsync().Result
    //     // );
    //     // var imageUrl = doc.RootElement.GetProperty("data").GetProperty("display_url").GetString();
    //     // var deleteUrl = doc.RootElement.GetProperty("data").GetProperty("delete_url").GetString();

    //     // 4. Trả về đúng string
    //     return response.Content.ReadFromJsonAsync<ImageDto>().Result!;
    // }

    // public async Task DeleteImageAsync(List<string> deleteUrls)
    // {
    //     // if (deleteUrls == null || !deleteUrls.Any())
    //     // {
    //     //     throw new ArgumentException("Delete URL list is null or empty", nameof(deleteUrls));
    //     // }

    //     var apiKey = "b9a4d9322875f65c9bf9fbedfc2a310a";
    //     var url = $"https://api.imgbb.com/1/upload?key={apiKey}";

    //     using var client = new HttpClient();

    //     foreach (var deleteUrl in deleteUrls)
    //     {
    //         var response = await client.DeleteAsync(deleteUrl);
    //         if (!response.IsSuccessStatusCode)
    //         {
    //             throw new Exception("Failed to delete image from imgbb.");
    //         }
    //     }
    // }
}
