using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;
using WebApi.Helpers;

namespace WebApi.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudinaryHelper _cloudinaryHelper;

    public CommentService(IUnitOfWork unitOfWork, CloudinaryHelper cloudinaryHelper)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryHelper = cloudinaryHelper;
    }

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        return await _unitOfWork.Comments.GetByIdAsync(commentId);
    }

    public async Task<List<GetChildCommentsResponse>> GetChildCommentList(
        Guid commentId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var comments = await _unitOfWork
            .Comments.GetQuery()
            .Where(c =>
                c.ParentCommentId == commentId
                && (cursor == null || c.CreatedAt < cursor)
                && c.DeletedAt == null
            )
            .Select(c => new GetChildCommentsResponse
            {
                Id = c.Id,
                Content = c.Content,
                AccountId = c.AccountId,
                AccountName = c.Account.DisplayName,
                ReplyAccountName =
                    c.ReplyAccount != null ? c.ReplyAccount.DisplayName : string.Empty,
                CreatedAt = c.CreatedAt,
                CommentPictures = c.Pictures.Select(cp => cp.Link).ToList(),
                LikeCount = c.CommentLikes.Count(),
                CommentCount = c.Replies.Count(),
                IsLiked = c.CommentLikes.Any(cl => cl.AccountId == accountId),
            })
            .OrderByDescending(c => c.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        return comments.ToList();
    }

    public async Task<bool> LikeCommentAsync(Guid commentId, Guid accountId)
    {
        var existingLike = await _unitOfWork.CommentLikes.GetByAccountAndCommentAsync(
            accountId,
            commentId
        );

        if (existingLike == null)
        {
            _unitOfWork.CommentLikes.Add(
                new CommentLike
                {
                    CommentId = commentId,
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                }
            );
            await _unitOfWork.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> CancelLikeCommentAsync(Guid commentId, Guid accountId)
    {
        var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingComment != null && existingComment.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.CommentLikes.GetByAccountAndCommentAsync(
                accountId,
                commentId
            );

            if (existingLike != null)
            {
                _unitOfWork.CommentLikes.Remove(existingLike);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        return false;
    }

    public async Task<CreateCommentResponse> AddCommentAsync(
        Guid accountId,
        CreateCommentRequest request,
        List<ImageDto> images
    )
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            AccountId = accountId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            ReplyAccountId = request.ReplyAccountId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Comments.Add(comment);

        var picture = images
            .Select(i => new Picture
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                Link = i.Link,
                PublicId = i.PublicId,
            })
            .ToList();

        _unitOfWork.Pictures.AddRange(picture);

        await _unitOfWork.SaveChangesAsync();

        return new CreateCommentResponse
        {
            Id = comment.Id,
            AccountId = accountId,
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            PostId = comment.PostId,
            ReplyAccountId = comment.ReplyAccountId,
            Pictures = images.Select(i => i.Link).ToList(),
            CreatedAt = comment.CreatedAt,
        };
    }

    public async Task<UpdateCommentResponse?> UpdateCommentAsync(
        Guid commentId,
        UpdateCommentRequest request,
        List<ImageDto>? images
    )
    {
        var existingComment = await _unitOfWork
            .Comments.GetQuery()
            .Include(c => c.Pictures)
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (existingComment == null || existingComment.DeletedAt != null)
        {
            return null;
        }

        existingComment.Content = request.Content;
        existingComment.UpdatedAt = DateTime.UtcNow;

        if (images != null)
        {
            var existingPictures = await _unitOfWork.Pictures.GetByCommentIdAsync(commentId);

            _unitOfWork.Pictures.RemoveRange(existingPictures);

            var picture = images
                .Select(i => new Picture
                {
                    Id = Guid.NewGuid(),
                    CommentId = commentId,
                    Link = i.Link,
                    PublicId = i.PublicId,
                })
                .ToList();

            _unitOfWork.Pictures.AddRange(picture);
        }

        await _unitOfWork.SaveChangesAsync();

        return new UpdateCommentResponse
        {
            Id = commentId,
            Content = request.Content,
            Pictures = existingComment.Pictures.Select(p => p.Link).ToList(),
            UpdatedAt = existingComment.UpdatedAt,
        };
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId)
    {
        var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingComment == null || existingComment.DeletedAt != null)
        {
            return false;
        }

        existingComment.DeletedAt = DateTime.UtcNow;

        var existingPictures = await _unitOfWork.Pictures.GetByCommentIdAsync(commentId);
        _unitOfWork.Pictures.RemoveRange(existingPictures);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<ImageDto>> AddImageAsync(List<IFormFile> files)
    {
        var uploadedImages = new List<ImageDto>();

        foreach (var file in files)
        {
            var res = await _cloudinaryHelper.Upload(file);
            uploadedImages.Add(new ImageDto { Link = res.Link, PublicId = res.PublicId });
        }

        return uploadedImages;
    }

    public async Task<List<ImageDto>> UpdateImageAsync(Guid commentId, List<IFormFile> files)
    {
        var uploadedImages = new List<ImageDto>();

        await DeleteImageAsync(commentId);

        if (files == null || !files.Any())
            return [];

        foreach (var file in files)
        {
            if (file != null)
            {
                var res = await _cloudinaryHelper.Upload(file);

                uploadedImages.Add(new ImageDto { Link = res.Link, PublicId = res.PublicId });
            }
        }

        return uploadedImages;
    }

    public async Task<bool> DeleteImageAsync(Guid commentId)
    {
        var oldFiles = await _unitOfWork.Pictures.GetByCommentIdAsync(commentId);

        foreach (var file in oldFiles)
        {
            await _cloudinaryHelper.Delete(file.PublicId);
        }

        return true;
    }
}
