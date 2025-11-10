using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;

namespace WebApi.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IBaseRepository _repository;

    public CommentService(IBaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        return await _repository.Comments.GetByIdAsync(commentId);
    }

    public async Task<List<GetChildCommentsResponse>> GetChildCommentList(
        Guid commentId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var comments = await _repository
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
        var existingLike = await _repository.CommentLikes.GetByAccountAndCommentAsync(
            accountId,
            commentId
        );

        if (existingLike == null)
        {
            _repository.CommentLikes.Add(
                new CommentLike
                {
                    CommentId = commentId,
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                }
            );
            await _repository.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> CancelLikeCommentAsync(Guid commentId, Guid accountId)
    {
        var existingComment = await _repository.Comments.GetByIdAsync(commentId);
        if (existingComment != null && existingComment.DeletedAt == null)
        {
            var existingLike = await _repository.CommentLikes.GetByAccountAndCommentAsync(
                accountId,
                commentId
            );

            if (existingLike != null)
            {
                _repository.CommentLikes.Remove(existingLike);
                await _repository.SaveChangesAsync();
            }

            return true;
        }

        return false;
    }
}
