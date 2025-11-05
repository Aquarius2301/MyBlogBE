using System;
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
        var query = _unitOfWork.Posts.GetQuery()
         .Where(x => x.DeletedAt == null && (cursor == null ? x.CreatedAt < DateTime.UtcNow : x.CreatedAt < cursor))
         .Select(x => new GetPostsResponse
         {
             Id = x.Id,
             Link = x.Link,
             Content = x.Content,
             AccountId = x.AccountId,
             AccountName = x.Account.DisplayName,
             CreatedAt = x.CreatedAt,
             PostPicture = x.PostPictures.Select(pp => pp.Link).ToList(),
             LatestComment = x.Comments.Where(x => x.ParentCommentId == null).OrderByDescending(c => c.CreatedAt).Select(c => new PostLatestComment
             {
                 Username = c.Account.Username,
                 DisplayName = c.Account.DisplayName,
                 Content = c.Content,
                 CreatedAt = c.CreatedAt
             }).FirstOrDefault(),
             LikeCount = x.PostLikes.Count(),
             CommentCount = x.Comments.Count(),
             IsFollowing = x.Account.Followers.Any(f => f.AccountId == userId && f.FollowingId == x.AccountId),
             IsLiked = x.PostLikes.Any(pl => pl.AccountId == userId),
         }).OrderByDescending(x => x.CreatedAt).Take(pageSize);

        var posts = await query.ToListAsync();


        return posts.OrderByDescending(x => x.Score).ToList();
    }
}
