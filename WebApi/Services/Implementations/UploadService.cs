using System;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using WebApi.Dtos;
using WebApi.Helpers;

namespace WebApi.Services;

public class UploadService : IUploadService
{
    private readonly UploadHelper _uploadHelper;
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(UploadHelper uploadHelper, IUnitOfWork unitOfWork)
    {
        _uploadHelper = uploadHelper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<string>> UploadFiles(IFormFile[] files)
    {
        List<ImageDto> res = [];

        foreach (var file in files)
            res.Add(await _uploadHelper.UploadImageAsync(file));

        _unitOfWork.Pictures.AddRange(
            res.Select(r => new Picture { PublicId = r.PublicId, Link = r.Link })
        );

        await _unitOfWork.SaveChangesAsync();

        return res.Select(r => r.Link).ToList();
    }
}
