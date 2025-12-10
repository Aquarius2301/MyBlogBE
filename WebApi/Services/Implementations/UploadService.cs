using System;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using WebApi.Dtos;
using WebApi.Helpers;

namespace WebApi.Services;

public class UploadService : IUploadService
{
    private readonly CloudinaryHelper _cloudinary;
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(CloudinaryHelper cloudinary, IUnitOfWork unitOfWork)
    {
        _cloudinary = cloudinary;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<string>> UploadFiles(IFormFile[] files)
    {
        var res = await _cloudinary.UploadImages(files);

        _unitOfWork.Pictures.AddRange(
            res.Select(r => new Picture { PublicId = r.PublicId, Link = r.Link })
        );

        await _unitOfWork.SaveChangesAsync();

        return res.Select(r => r.Link).ToList();
    }
}
