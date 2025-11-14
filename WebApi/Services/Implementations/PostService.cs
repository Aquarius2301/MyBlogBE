using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Settings;

namespace WebApi.Services.Implementations;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BaseSettings _settings;

    public PostService(IUnitOfWork unitOfWork, IOptions<BaseSettings> options)
    {
        _unitOfWork = unitOfWork;
        _settings = options.Value;
    }

    public async Task<Post?> GetByIdAsync(Guid commentId)
    {
        return await _unitOfWork.Posts.GetByIdAsync(commentId);
    }

    public async Task<List<GetPostsResponse>> GetPostsListAsync(
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var query = _unitOfWork
            .Posts.GetQuery()
            .Where(x =>
                x.DeletedAt == null
                && (cursor == null ? x.CreatedAt < DateTime.UtcNow : x.CreatedAt < cursor)
            )
            .Select(x => new GetPostsResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                CreatedAt = x.CreatedAt,
                PostPictures = x.Pictures.Select(pp => pp.Link).ToList(),
                LatestComment = x
                    .Comments.Where(x => x.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new PostLatestComment
                    {
                        Username = c.Account.Username,
                        DisplayName = c.Account.DisplayName,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                    })
                    .FirstOrDefault(),
                LikeCount = x.PostLikes.Count(),
                CommentCount = x.Comments.Count(),
                IsFollowing = x.Account.Followers.Any(f =>
                    f.AccountId == accountId && f.FollowingId == x.AccountId
                ),
                IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == accountId),
            })
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize);

        var posts = await query.ToListAsync();

        return posts.OrderByDescending(x => x.Score).ToList();
    }

    public async Task<List<GetMyPostsResponse>> GetMyPostsListAsync(
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var query = _unitOfWork
            .Posts.GetQuery()
            .Where(x =>
                x.DeletedAt == null
                && x.AccountId == accountId
                && (cursor == null ? x.CreatedAt < DateTime.UtcNow : x.CreatedAt < cursor)
            )
            .Select(x => new GetMyPostsResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                CreatedAt = x.CreatedAt,
                PostPicture = x.Pictures.Select(pp => pp.Link).ToList(),
                LatestComment = x
                    .Comments.Where(x => x.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new PostLatestComment
                    {
                        Username = c.Account.Username,
                        DisplayName = c.Account.DisplayName,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                    })
                    .FirstOrDefault(),
                LikeCount = x.PostLikes.Count(),
                CommentCount = x.Comments.Count(),
                IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == accountId),
            })
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize);

        return await query.ToListAsync();
    }

    public async Task<GetPostDetailResponse?> GetPostByLinkAsync(string link, Guid accountId)
    {
        var post = await _unitOfWork
            .Posts.GetQuery()
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
                IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == accountId),
            })
            .FirstOrDefaultAsync();

        return post;
    }

    public async Task LikePostAsync(Guid postId, Guid accountId)
    {
        var existingLike = await _unitOfWork.PostLikes.GetByAccountAndPostAsync(accountId, postId);

        if (existingLike == null)
        {
            _unitOfWork.PostLikes.Add(
                new PostLike
                {
                    PostId = postId,
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                }
            );
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task CancelLikePostAsync(Guid postId, Guid accountId)
    {
        var existingLike = await _unitOfWork.PostLikes.GetByAccountAndPostAsync(accountId, postId);

        if (existingLike != null)
        {
            _unitOfWork.PostLikes.Remove(existingLike);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<GetCommentsResponse>> GetPostCommentsList(
        Guid postId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var comments = await _unitOfWork
            .Comments.GetQuery()
            .Where(c =>
                c.PostId == postId
                && c.ParentCommentId == null
                && (cursor == null || c.CreatedAt < cursor)
                && c.DeletedAt == null
            )
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
                IsLiked = c.CommentLikes.Any(cl => cl.AccountId == accountId),
            })
            .OrderByDescending(c => c.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        return comments.OrderByDescending(c => c.Score).ToList();
    }

    public async Task<CreatePostResponse> AddPostAsync(CreatePostRequest request, Guid accountId)
    {
        string link;

        do
        {
            link = StringHelper.GenerateRandomString(_settings.TokenLength);
        } while (await _unitOfWork.Posts.GetByLinkAsync(link) != null);

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            Link = link,
            Content = request.Content,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Posts.Add(newPost);
        await _unitOfWork.SaveChangesAsync();

        return new CreatePostResponse
        {
            Id = newPost.Id,
            Link = newPost.Link,
            Content = newPost.Content,
        };
    }
}
