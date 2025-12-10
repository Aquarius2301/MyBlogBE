namespace WebApi.Dtos;

public class UploadRequest
{
    public IFormFile[] Pictures { get; set; } = null!;
}

public class ImageDto
{
    public string PublicId { get; set; } = null!;
    public string Link { get; set; } = null!;
}
