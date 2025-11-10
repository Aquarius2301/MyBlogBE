using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Settings;

namespace WebApi.Helpers;

/// <summary>
/// Helper class for uploading and deleting images using Cloudinary.
/// </summary>
public class CloudinaryHelper
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;

    /// <summary>
    /// Initializes the Cloudinary client using provided settings.
    /// </summary>
    /// <param name="options">Cloudinary configuration options.</param>
    public CloudinaryHelper(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    /// <summary>
    /// Uploads an image file to Cloudinary.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>
    /// Returns an <see cref="ImageDto"/> containing the uploaded image key and URL.
    /// </returns>
    /// <exception cref="Exception">Thrown if the upload fails.</exception>
    public ImageDto Upload(IFormFile file)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
        };

        var uploadResult = _cloudinary.Upload(uploadParams);

        if (uploadResult != null)
        {
            return new ImageDto
            {
                Key = uploadResult.PublicId,
                Link = uploadResult.SecureUrl.ToString(),
            };
        }
        else
        {
            throw new Exception("Error when uploading image!");
        }
    }

    /// <summary>
    /// Deletes an image from Cloudinary using its public ID.
    /// </summary>
    /// <param name="publicId">The public ID of the image to delete.</param>
    /// <returns>
    /// True if the deletion was successful, otherwise False.
    /// </returns>
    public bool Delete(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = _cloudinary.Destroy(deletionParams);

        return result.Result == "ok";
    }
}
