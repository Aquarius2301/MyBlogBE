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

    public async Task<List<GetCommentsResponse>?> GetCommentList(Guid postId, DateTime? cursor, Guid userId, int pageSize)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(postId);

        if (post != null && post.DeletedAt == null)
        {
            var comments =
             await _unitOfWork.Comments.GetQuery()
                 .Where(c => c.PostId == postId && c.ParentCommentId == null && (cursor == null || c.CreatedAt < cursor) &&
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

        return null;
    }

    public async Task<List<GetChildCommentsResponse>?> GetChildCommentList(Guid commentId, DateTime? cursor, Guid userId, int pageSize)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);

        if (comment != null && comment.DeletedAt == null)
        {
            var comments =
                await _unitOfWork.Comments.GetQuery()
                    .Where(c => c.ParentCommentId == commentId && (cursor == null || c.CreatedAt < cursor) &&
                                c.DeletedAt == null)
                    .Select(c => new GetChildCommentsResponse
                    {
                        Id = c.Id,
                        Content = c.Content,
                        AccountId = c.AccountId,
                        AccountName = c.Account.DisplayName,
                        ReplyAccountName = c.ReplyAccount != null
                                               ? c.ReplyAccount.DisplayName
                                               : string.Empty,
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

        return null;
    }

    public async Task<bool> LikeCommentAsync(Guid commentId, Guid userId)
    {
        var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);

        if (existingComment != null && existingComment.DeletedAt == null)
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
        }

        return false;
    }

    public async Task<bool> CancelLikeCommentAsync(Guid commentId, Guid userId)
    {
        var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (existingComment != null && existingComment.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.CommentLikes.GetCommentLikeByPostAndAccountAsync(userId, commentId);

            if (existingLike != null)
            {
                await _unitOfWork.CommentLikes.DeleteAsync(existingLike);

                return true;
            }
        }

        return false;
    }
}
