using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Middlewares;

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
        var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingComment == null)
        {
            return false;
        }

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

        if (existingComment == null)
        {
            return false;
        }

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

    public async Task<CreateCommentResponse> AddCommentAsync(
        Guid accountId,
        CreateCommentRequest request
    )
    {
        if (request.ParentCommentId != null)
        {
            var parentComment = await _unitOfWork.Comments.GetByIdAsync(
                request.ParentCommentId.Value
            );
            if (parentComment == null)
            {
                throw new AppException("Parent comment does not exist", 404);
            }
        }

        if (request.ReplyAccountId != null)
        {
            var parentComment = await _unitOfWork.Accounts.GetByIdAsync(
                request.ReplyAccountId.Value
            );

            if (parentComment == null)
            {
                throw new AppException("Reply account does not exist", 404);
            }
        }

        var post = await _unitOfWork.Posts.GetByIdAsync(request.PostId);

        if (post == null)
        {
            throw new AppException("Post does not exist", 404);
        }

        // Upload images to Cloudinary
        var images = await _cloudinaryHelper.UploadImages(request.Images);

        // Add comment to database
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

        // Add pictures to database
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
        Guid accountId
    )
    {
        var uploadedImages = new List<ImageDto>();
        var images = new List<ImageDto>();

        var existingComment = await _unitOfWork
            .Comments.GetQuery()
            .Include(c => c.Pictures)
            .FirstOrDefaultAsync(c =>
                c.Id == commentId && c.AccountId == accountId && c.DeletedAt == null
            );

        if (existingComment == null)
        {
            return null;
        }

        // if ClearImages is true,  delete all existing images and upload new ones.
        // if ClearImages is false, keep existing images (only update the content).
        if (request.ClearImages)
        {
            await _cloudinaryHelper.DeleteImages(
                existingComment.Pictures.Select(p => p.PublicId).ToList()
            );

            images = await _cloudinaryHelper.UploadImages(request.Images);
        }

        // Update comment content
        existingComment.Content = request.Content;
        existingComment.UpdatedAt = DateTime.UtcNow;

        if (request.ClearImages)
        {
            // Delete existing pictures from database
            var existingPictures = await _unitOfWork.Pictures.GetByCommentIdAsync(commentId);

            _unitOfWork.Pictures.RemoveRange(existingPictures);

            // Add new pictures to database
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

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid accountId)
    {
        var existingComment = await _unitOfWork
            .Comments.GetQuery()
            .Include(c => c.Pictures)
            .FirstOrDefaultAsync(c =>
                c.Id == commentId && c.AccountId == accountId && c.DeletedAt == null
            );

        if (existingComment == null)
        {
            return false;
        }

        await _cloudinaryHelper.DeleteImages(
            existingComment.Pictures.Select(p => p.PublicId).ToList()
        );

        existingComment.DeletedAt = DateTime.UtcNow;

        var existingPictures = await _unitOfWork.Pictures.GetByCommentIdAsync(commentId);
        _unitOfWork.Pictures.RemoveRange(existingPictures);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
