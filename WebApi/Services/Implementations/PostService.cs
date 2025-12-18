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
            .Where(x => x.DeletedAt == null && (cursor == null || x.CreatedAt < cursor))
            .Select(x => new GetPostsResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                AccountUsername = x.Account.Username,
                AccountAvatar = x.Account.Picture != null ? x.Account.Picture.Link : "",
                IsOwner = x.AccountId == accountId,
                CreatedAt = x.CreatedAt,
                PostPictures = x.Pictures.Select(pp => pp.Link).ToList(),
                LatestComment = x
                    .Comments.Where(x => x.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new PostLatestComment
                    {
                        Avatar = c.Account.Picture != null ? c.Account.Picture.Link : "",
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

        return posts.ToList();
    }

    public async Task<List<GetPostsResponse>> GetMyPostsListAsync(
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
            .Select(x => new GetPostsResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                AccountUsername = x.Account.Username,
                AccountAvatar = x.Account.Picture != null ? x.Account.Picture.Link : "",
                IsOwner = true,
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
                IsLiked = x.PostLikes.Any(pl => pl.PostId == x.Id && pl.AccountId == accountId),
            })
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize);

        return await query.ToListAsync();
    }

    public async Task<List<GetPostsResponse>> GetPostsByUsername(
        string username,
        DateTime? cursor,
        Guid accountId,
        int pageSize
    )
    {
        var query = _unitOfWork
            .Posts.GetQuery()
            .Where(x =>
                x.DeletedAt == null
                && x.Account.Username == username
                && (cursor == null ? x.CreatedAt < DateTime.UtcNow : x.CreatedAt < cursor)
            )
            .Select(x => new GetPostsResponse
            {
                Id = x.Id,
                Link = x.Link,
                Content = x.Content,
                AccountId = x.AccountId,
                AccountName = x.Account.DisplayName,
                AccountUsername = x.Account.Username,
                AccountAvatar = x.Account.Picture != null ? x.Account.Picture.Link : "",
                IsOwner = x.AccountId == accountId,
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
                AccountAvatar = x.Account.Picture != null ? x.Account.Picture.Link : "",
                IsOwner = x.AccountId == accountId,
                CreatedAt = x.CreatedAt,
                PostPictures = x.Pictures.Select(pp => pp.Link).ToList(),
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
        if (existingPost == null)
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
                AccountAvatar = c.Account.Picture != null ? c.Account.Picture.Link : "",
                CreatedAt = c.CreatedAt,
                CommentPictures = c.Pictures.Select(cp => cp.Link).ToList(),
                LikeCount = c.CommentLikes.Count(),
                CommentCount = c.Replies.Count(),
                IsLiked = c.CommentLikes.Any(cl => cl.AccountId == accountId),
            })
            .OrderByDescending(c => c.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        return comments;
    }

    public async Task<CreatePostResponse?> AddPostAsync(CreatePostRequest request, Guid accountId)
    {
        // 1. Tối ưu truy vấn Account: Dùng AsNoTracking và Select để chỉ lấy dữ liệu cần thiết
        var existingAccount = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking() // Không cần theo dõi thay đổi vì chỉ để đọc
            .Where(a =>
                a.Id == accountId
                && a.DeletedAt == null
                && a.Status == BusinessObject.Enums.StatusType.Active
            )
            .Select(a => new
            {
                a.DisplayName,
                // Xử lý null check ngay trong query để tránh lỗi
                Avatar = a.Picture != null ? a.Picture.Link : "",
            })
            .FirstOrDefaultAsync();

        if (existingAccount == null)
        {
            return null;
        }

        // 2. Generate Link (Giữ nguyên logic nhưng đảm bảo field Link trong DB đã đánh Index)
        string link;
        do
        {
            link = StringHelper.GenerateRandomString(_settings.TokenLength);
        } while (await _unitOfWork.Posts.GetByLinkAsync(link) != null);

        var createTime = DateTime.UtcNow;
        var postId = Guid.NewGuid();

        var newPost = new Post
        {
            Id = postId,
            Link = link,
            Content = request.Content,
            AccountId = accountId,
            CreatedAt = createTime,
        };

        _unitOfWork.Posts.Add(newPost);

        // 3. Tối ưu xử lý Pictures: Thay thế vòng lặp foreach (N+1 query) bằng 1 query duy nhất
        // Giả sử request.Pictures là List<string> chứa Link của ảnh
        if (request.Pictures != null && request.Pictures.Any())
        {
            var pictures = await _unitOfWork
                .Pictures.GetQuery()
                .Where(p => request.Pictures.Contains(p.Link))
                .ToListAsync();

            foreach (var picture in pictures)
            {
                picture.PostId = postId;
                // EF Core tự động track changes cho các entity này
            }

            // Lưu ý: Nếu logic yêu cầu bắt buộc tất cả ảnh trong request phải tồn tại,
            // bạn cần check: if (pictures.Count != request.Pictures.Count) -> throw Exception
        }

        // 4. SaveChanges một lần duy nhất cho cả Post và Pictures
        await _unitOfWork.SaveChangesAsync();

        // 5. Trả về kết quả từ dữ liệu có sẵn trong memory, KHÔNG query lại DB
        return new CreatePostResponse
        {
            Id = newPost.Id,
            AccountAvatar = existingAccount.Avatar,
            AccountName = existingAccount.DisplayName,
            Link = newPost.Link,
            Content = newPost.Content,
            // Dùng lại list pictures đã lấy ở bước 3, không cần query lại
            PostPictures = request.Pictures ?? new List<string>(),
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
            .Include(p => p.Account)
                .ThenInclude(a => a.Picture)
            .FirstOrDefaultAsync(p =>
                p.Id == postId && p.AccountId == accountId && p.DeletedAt == null
            );
        if (existingPost == null)
        {
            return null;
        }

        // var pictureLinks = new List<ImageDto>();

        // if (request.ClearPictures)
        // {
        var existingPictures = await _unitOfWork.Pictures.GetByPostIdAsync(postId);

        foreach (var picture in existingPictures)
        {
            picture.PostId = null;
        }

        // _unitOfWork.Pictures.AddRange(
        //     request.Pictures.Select(pl => new Picture
        //     {
        //         Id = Guid.NewGuid(),
        //         PostId = existingPost.Id,
        //         PublicId = pl.PublicId,
        //         Link = pl.Link,
        //     })
        // );
        // }

        var updateTime = DateTime.UtcNow;

        existingPost.Content = request.Content;
        existingPost.UpdatedAt = updateTime;

        if (request.Pictures != null && request.Pictures.Any())
        {
            var pictures = await _unitOfWork
                .Pictures.GetQuery()
                .Where(p => request.Pictures.Contains(p.Link))
                .ToListAsync();

            foreach (var picture in pictures)
            {
                picture.PostId = postId;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new UpdatePostResponse
        {
            Id = existingPost.Id,
            AccountAvatar =
                existingPost.Account.Picture != null ? existingPost.Account.Picture.Link : "",
            AccountName = existingPost.Account.DisplayName,
            Link = existingPost.Link,
            Content = request.Content,
            PostPictures = existingPost.Pictures.Select(x => x.Link).ToList(),
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

        foreach (var picture in existingPictures)
        {
            picture.PostId = null;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<string>> UploadPostImagesAsync(
        UploadPostImageRequest request,
        Guid accountId
    )
    {
        var pics = new List<ImageDto>();
        try
        {
            var existingAccount = await _unitOfWork
                .Accounts.GetQuery()
                .FirstOrDefaultAsync(a =>
                    a.Id == accountId
                    && a.DeletedAt == null
                    && a.Status == BusinessObject.Enums.StatusType.Active
                );

            pics = await _cloudinaryHelper.UploadImages(request.Pictures);

            _unitOfWork.Pictures.AddRange(
                pics.Select(pl => new Picture
                {
                    Id = Guid.NewGuid(),
                    PublicId = pl.PublicId,
                    Link = pl.Link,
                })
            );

            await _unitOfWork.SaveChangesAsync();

            return pics.Select(x => x.Link).ToList();
        }
        catch (Exception)
        {
            await _cloudinaryHelper.DeleteImages(pics.Select(x => x.PublicId).ToList());
            throw;
        }
    }
}
