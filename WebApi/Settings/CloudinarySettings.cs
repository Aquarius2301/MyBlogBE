namespace WebApi.Settings;

/// <summary>
/// Configuration settings for Cloudinary integration.
/// </summary>
public class CloudinarySettings
{
    /// <summary>
    /// The Cloudinary cloud name.
    /// </summary>
    public string CloudName { get; set; } = null!;

    /// <summary>
    /// The Cloudinary API key.
    /// </summary>
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// The Cloudinary API secret key.
    /// </summary>
    public string ApiSecret { get; set; } = null!;
}
