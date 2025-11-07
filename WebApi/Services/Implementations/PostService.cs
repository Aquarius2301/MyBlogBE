using System;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;

    public PostService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetPostsResponse>> GetPostsListAsync(DateTime? cursor, Guid userId, int pageSize)
    {
        var query =
            _unitOfWork.Posts.GetQuery()
                .Where(x => x.DeletedAt == null &&
                            (cursor == null ? x.CreatedAt < DateTime.UtcNow
                                            : x.CreatedAt < cursor))
                .Select(x => new GetPostsResponse
                {
                    Id = x.Id,
                    Link = x.Link,
                    Content = x.Content,
                    AccountId = x.AccountId,
                    AccountName = x.Account.DisplayName,
                    CreatedAt = x.CreatedAt,
                    PostPictures = x.Pictures.Select(pp => pp.Link).ToList(),
                    LatestComment =
                        x.Comments.Where(x => x.ParentCommentId == null)
                            .OrderByDescending(c => c.CreatedAt)
                            .Select(c => new PostLatestComment
                            {
                                Username = c.Account.Username,
                                DisplayName = c.Account.DisplayName,
                                Content = c.Content,
                                CreatedAt = c.CreatedAt
                            })
                            .FirstOrDefault(),
                    LikeCount = x.PostLikes.Count(),
                    CommentCount = x.Comments.Count(),
                    IsFollowing = x.Account.Followers.Any(
                        f => f.AccountId == userId &&
                             f.FollowingId == x.AccountId),
                    IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == userId),
                })
                .OrderByDescending(x => x.CreatedAt)
                .Take(pageSize);

        var posts = await query.ToListAsync();

        return posts.OrderByDescending(x => x.Score).ToList();
    }

    public async Task<List<GetMyPostsResponse>> GetMyPostsListAsync(DateTime? cursor, Guid userId, int pageSize)
    {
        var query =
            _unitOfWork.Posts.GetQuery()
                .Where(x => x.DeletedAt == null && x.AccountId == userId &&
                            (cursor == null ? x.CreatedAt < DateTime.UtcNow
                                            : x.CreatedAt < cursor))
                .Select(x => new GetMyPostsResponse
                {
                    Id = x.Id,
                    Link = x.Link,
                    Content = x.Content,
                    AccountId = x.AccountId,
                    AccountName = x.Account.DisplayName,
                    CreatedAt = x.CreatedAt,
                    PostPicture = x.Pictures.Select(pp => pp.Link).ToList(),
                    LatestComment =
                        x.Comments.Where(x => x.ParentCommentId == null)
                            .OrderByDescending(c => c.CreatedAt)
                            .Select(c => new PostLatestComment
                            {
                                Username = c.Account.Username,
                                DisplayName = c.Account.DisplayName,
                                Content = c.Content,
                                CreatedAt = c.CreatedAt
                            })
                            .FirstOrDefault(),
                    LikeCount = x.PostLikes.Count(),
                    CommentCount = x.Comments.Count(),
                    IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == userId),
                })
                .OrderByDescending(x => x.CreatedAt)
                .Take(pageSize);

        return await query.ToListAsync();
    }

    public async Task<GetPostDetailResponse?> GetPostByLinkAsync(string link, Guid userId)
    {
        var post = await _unitOfWork.Posts.GetQuery()
            .Where(x => x.Link == link && x.DeletedAt == null)
            .Select(x => new GetPostDetailResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                CreatedAt = x.CreatedAt,
                PostPicture = x.Pictures.Select(pp => pp.Link).ToList(),
                LikeCount = x.PostLikes.Count(),
                CommentCount = x.Comments.Count(),
                IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == userId),
            })
            .FirstOrDefaultAsync();

        return post;
    }

    public async Task<bool> LikePostAsync(Guid postId, Guid userId)
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (existingPost != null && existingPost.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.PostLikes.GetPostLikeByPostAndAccountAsync(userId, postId);

            if (existingLike == null)
            {
                await _unitOfWork.PostLikes.AddAsync(new PostLike
                {
                    PostId = postId,
                    AccountId = userId,
                    CreatedAt = DateTime.UtcNow
                });

                return true;
            }
        }

        return false;
    }

    public async Task<bool> CancelLikePostAsync(Guid postId, Guid userId)
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (existingPost != null && existingPost.DeletedAt == null)
        {
            var existingLike = await _unitOfWork.PostLikes.GetPostLikeByPostAndAccountAsync(userId, postId);

            if (existingLike != null)
            {
                await _unitOfWork.PostLikes.DeleteAsync(existingLike);

                return true;
            }
        }

        return false;
    }
}
