using System;

namespace WebApi.Services;

public interface IUploadService
{
    Task<List<string>> UploadFiles(IFormFile[] files);
}
