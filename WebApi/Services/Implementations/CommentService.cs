using System;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class CommentService : ICommentService
{
    private IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetCommentsResponse>> GetCommentList(Guid postId, Guid userId, int pageSize)
    {
        var comments =
            await _unitOfWork.Comments.GetQuery()
                .Where(c => c.PostId == postId && c.ParentCommentId == null &&
                            c.DeletedAt == null)
                .Select(c => new GetCommentsResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    AccountId = c.AccountId,
                    AccountName = c.Account.DisplayName,
                    CreatedAt = c.CreatedAt,
                    CommentPictures = c.Pictures.Select(cp => cp.Link).ToList(),
                    LikeCount = c.CommentLikes.Count(),
                    CommentCount = c.Replies.Count(),
                    IsLiked = c.CommentLikes.Any(cl => cl.AccountId == userId)
                })
                .OrderByDescending(c => c.CreatedAt)
                .Take(pageSize)
                .ToListAsync();

        return comments.OrderByDescending(c => c.Score).ToList();
    }

    public async Task<List<GetChildCommentsResponse>> GetChildCommentList(Guid commentId, Guid userId, int pageSize)
    {
        var comments =
            await _unitOfWork.Comments.GetQuery()
                .Where(c => c.ParentCommentId == commentId &&
                            c.DeletedAt == null)
                .Select(c => new GetChildCommentsResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    AccountId = c.AccountId,
                    AccountName = c.Account.DisplayName,
                    ReplyAccountName = c.ReplyAccount != null ? c.ReplyAccount.DisplayName : string.Empty,
                    CreatedAt = c.CreatedAt,
                    CommentPictures = c.Pictures.Select(cp => cp.Link).ToList(),
                    LikeCount = c.CommentLikes.Count(),
                    CommentCount = c.Replies.Count(),
                    IsLiked = c.CommentLikes.Any(cl => cl.AccountId == userId)
                })
                .OrderByDescending(c => c.CreatedAt)
                .Take(pageSize)
                .ToListAsync();

        return comments.ToList();
    }

    public async Task<bool?> LikeCommentAsync(Guid commentId, Guid userId)
    {
        var existingPost = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingPost != null && existingPost.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.CommentLikes.GetCommentLikeByPostAndAccountAsync(userId, commentId);

            if (existingLike == null)
            {
                await _unitOfWork.CommentLikes.AddAsync(new CommentLike
                {
                    CommentId = commentId,
                    AccountId = userId,
                    CreatedAt = DateTime.UtcNow
                });

                return true;
            }

            return false;
        }
        return null;
    }

    public async Task<bool?> CancelLikeCommentAsync(Guid commentId, Guid userId)
    {
        var existingPost = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingPost != null && existingPost.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.CommentLikes.GetCommentLikeByPostAndAccountAsync(userId, commentId);

            if (existingLike != null)
            {
                await _unitOfWork.CommentLikes.DeleteAsync(existingLike);

                return true;
            }

            return false;
        }
        return null;
    }
}
