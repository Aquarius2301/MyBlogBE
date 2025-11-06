using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebApi.Dtos;

namespace WebApi.Helpers;

public class CloudinarySettings
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}

public class CloudinaryHelper
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;

    public CloudinaryHelper(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public ImageDto Upload(IFormFile file)
    {
        if (file == null)
        {
            throw new ArgumentException("No files to upload");
        }

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException($"File {file?.FileName} is null or empty");
        }

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.OpenReadStream())
        };

        var uploadResult = _cloudinary.Upload(uploadParams);

        if (uploadResult != null)
        {
            return new ImageDto
            {
                Key = uploadResult.PublicId,
                Link = uploadResult.SecureUrl.ToString()
            };
        }
        else
        {
            throw new Exception("Error when uploading image!");
        }
    }

    public bool Delete(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = _cloudinary.Destroy(deletionParams);

        return result.Result == "ok";
    }
}
