using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Settings;

namespace WebApi.Controllers;

[Authorize]
[Route("api/posts")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _service;
    private readonly ILanguageService _lang;
    private readonly JwtHelper _jwtHelper;
    private readonly CloudinaryHelper _cloudinaryHelper;
    private readonly BaseSettings _settings;

    public PostController(
        IPostService service,
        ILanguageService lang,
        JwtHelper jwtHelper,
        CloudinaryHelper cloudinaryHelper,
        IOptions<BaseSettings> options
    )
    {
        _service = service;
        _lang = lang;
        _jwtHelper = jwtHelper;
        _cloudinaryHelper = cloudinaryHelper;
        _settings = options.Value;
    }

    /// <summary>
    /// Retrieves a paginated list of all posts in the system.
    /// </summary>
    /// <param name="request">Pagination parameters including cursor and page size.</param>
    /// <returns>
    /// 200 - Returns paginated list of posts with cursor for next page.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("")]
    public async Task<IActionResult> GetPosts([FromQuery] PaginationRequest request)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var res = await _service.GetPostsListAsync(request.Cursor, user.Id, request.PageSize);

            return ApiResponse.Success(
                new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = request.PageSize,
                }
            );
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of posts created by the authenticated user.
    /// </summary>
    /// <param name="request">Pagination parameters including cursor and page size.</param>
    /// <returns>
    /// 200 - Returns paginated list of user's own posts with cursor for next page.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPosts([FromQuery] PaginationRequest request)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var res = await _service.GetMyPostsListAsync(request.Cursor, user.Id, request.PageSize);

            return ApiResponse.Success(
                new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = request.PageSize,
                }
            );
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a specific post by its unique link identifier.
    /// </summary>
    /// <param name="link">The unique link/slug identifier of the post.</param>
    /// <returns>
    /// 200 - Returns the post details if found.
    /// 404 - Returns error if post with specified link does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("link/{link}")]
    public async Task<IActionResult> GetPostsByLink(string link)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var res = await _service.GetPostByLinkAsync(link, user.Id);

            return res != null
                ? ApiResponse.Success(res)
                : ApiResponse.NotFound(_lang.Get("NoPost"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Likes a specific post.
    /// </summary>
    /// <param name="id">The unique identifier of the post to like.</param>
    /// <returns>
    /// 200 - Returns success if post is liked successfully.
    /// 400 - Returns error if post does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikePost(Guid id)
    {
        try
        {
            if (await _service.GetByIdAsync(id) == null)
            {
                return ApiResponse.BadRequest(_lang.Get("NoPost"));
            }

            var user = _jwtHelper.GetAccountInfo();

            return ApiResponse.Success();
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Removes a like from a specific post.
    /// </summary>
    /// <param name="id">The unique identifier of the post to unlike.</param>
    /// <returns>
    /// 200 - Returns success if like is removed successfully.
    /// 400 - Returns error if post does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpDelete("{id}/cancel-like")]
    public async Task<IActionResult> CancelLikePost(Guid id)
    {
        try
        {
            if (await _service.GetByIdAsync(id) == null)
            {
                return ApiResponse.BadRequest(_lang.Get("NoPost"));
            }

            var user = _jwtHelper.GetAccountInfo();

            await _service.CancelLikePostAsync(id, user.Id);

            return ApiResponse.Success();
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of comments for a specific post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="request">Pagination parameters including cursor and page size.</param>
    /// <returns>
    /// 200 - Returns paginated list of comments with cursor for next page.
    /// 404 - Returns error if post does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetPostComments(
        Guid postId,
        [FromQuery] PaginationRequest request
    )
    {
        try
        {
            if (await _service.GetByIdAsync(postId) == null)
            {
                return ApiResponse.NotFound(_lang.Get("NoPost"));
            }

            var user = _jwtHelper.GetAccountInfo();
            var res = await _service.GetPostCommentsList(
                postId,
                request.Cursor,
                user.Id,
                request.PageSize
            );

            return ApiResponse.Success(
                new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = request.PageSize,
                }
            );
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new post for the authenticated user.
    /// </summary>
    /// <param name="request">The post creation request containing content.</param>
    /// <returns>
    /// 201 - Returns the created post details upon successful creation.
    /// 400 - Returns error if the post content is empty.
    /// 403 - Returns error if the user's account status does not allow posting.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("")]
    public async Task<IActionResult> AddPost([FromBody] CreatePostRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return ApiResponse.BadRequest(_lang.Get("PostEmpty"));
            }

            var user = _jwtHelper.GetAccountInfo();

            if (user.StatusType != BusinessObject.Enums.StatusType.Active)
            {
                return ApiResponse.Forbidden(_lang.Get("InactiveAccountCannotCreatePost"));
            }

            var res = await _service.AddPostAsync(request, user.Id);

            return ApiResponse.Created(res, _lang.Get("PostCreated"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }
}
