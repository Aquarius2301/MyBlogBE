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
    private readonly CloudinaryHelper _cloudinaryHelper;

    public PostService(
        IUnitOfWork unitOfWork,
        IOptions<BaseSettings> options,
        CloudinaryHelper cloudinaryHelper
    )
    {
        _unitOfWork = unitOfWork;
        _settings = options.Value;
        _cloudinaryHelper = cloudinaryHelper;
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
                AccountAvatar = x.Account.Picture != null ? x.Account.Picture.Link : "",
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

    public async Task<bool> LikePostAsync(Guid postId, Guid accountId)
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (existingPost == null)
        {
            return false;
        }

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
        return true;
    }

    public async Task<bool> CancelLikePostAsync(Guid postId, Guid accountId)
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (existingPost == null)
        {
            return false;
        }

        var existingLike = await _unitOfWork.PostLikes.GetByAccountAndPostAsync(accountId, postId);

        if (existingLike != null)
        {
            _unitOfWork.PostLikes.Remove(existingLike);
            await _unitOfWork.SaveChangesAsync();
        }

        return true;
    }

    public async Task<List<GetCommentsResponse>?> GetPostCommentsList(
        Guid postId,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (existingPost == null || existingPost.AccountId != accountId)
        {
            return null;
        }

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

        var pictureLinks = await _cloudinaryHelper.UploadImages(request.Pictures);

        var createTime = DateTime.UtcNow;
        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            Link = link,
            Content = request.Content,
            AccountId = accountId,
            CreatedAt = createTime,
        };

        _unitOfWork.Posts.Add(newPost);

        var postPictures = pictureLinks.Select(pl => new Picture
        {
            Id = Guid.NewGuid(),
            PostId = newPost.Id,
            PublicId = pl.PublicId,
            Link = pl.Link,
        });
        _unitOfWork.Pictures.AddRange(postPictures);

        await _unitOfWork.SaveChangesAsync();

        return new CreatePostResponse
        {
            Id = newPost.Id,
            Link = newPost.Link,
            Content = newPost.Content,
            Pictures = postPictures.Select(pp => pp.Link).ToList(),
            CreatedAt = createTime,
        };
    }

    public async Task<UpdatePostResponse?> UpdatePostAsync(
        UpdatePostRequest request,
        Guid postId,
        Guid accountId
    )
    {
        var existingPost = await _unitOfWork
            .Posts.GetQuery()
            .Include(p => p.Pictures)
            .FirstOrDefaultAsync(p =>
                p.Id == postId && p.AccountId == accountId && p.DeletedAt == null
            );
        if (existingPost == null)
        {
            return null;
        }

        var pictureLinks = new List<ImageDto>();

        if (request.ClearPictures)
        {
            var existingPictures = await _unitOfWork.Pictures.GetByPostIdAsync(postId);

            await _cloudinaryHelper.DeleteImages(existingPictures.Select(x => x.PublicId).ToList());
            _unitOfWork.Pictures.RemoveRange(existingPictures);

            pictureLinks = await _cloudinaryHelper.UploadImages(request.Pictures);

            _unitOfWork.Pictures.AddRange(
                pictureLinks.Select(pl => new Picture
                {
                    Id = Guid.NewGuid(),
                    PostId = existingPost.Id,
                    PublicId = pl.PublicId,
                    Link = pl.Link,
                })
            );
        }

        var updateTime = DateTime.UtcNow;

        existingPost.Content = request.Content;
        existingPost.UpdatedAt = updateTime;

        await _unitOfWork.SaveChangesAsync();

        return new UpdatePostResponse
        {
            Id = existingPost.Id,
            Link = existingPost.Link,
            Content = request.Content,
            Pictures = existingPost.Pictures.Select(x => x.Link).ToList(),
            UpdatedAt = updateTime,
        };
    }

    public async Task<bool> DeletePostAsync(Guid postId, Guid accountId)
    {
        var existingPost = await _unitOfWork.Posts.GetByIdAsync(postId);

        if (existingPost == null || existingPost.AccountId != accountId)
        {
            return false;
        }

        existingPost.DeletedAt = DateTime.UtcNow;

        var existingPictures = await _unitOfWork.Pictures.GetByPostIdAsync(postId);

        await _cloudinaryHelper.DeleteImages(existingPictures.Select(x => x.PublicId).ToList());
        _unitOfWork.Pictures.RemoveRange(existingPictures);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
