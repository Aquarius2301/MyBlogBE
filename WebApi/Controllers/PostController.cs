using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[Route("api/posts")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _service;
    private readonly ILanguageService _lang;
    private readonly JwtHelper _jwtHelper;

    public PostController(IPostService service, ILanguageService lang, JwtHelper jwtHelper)
    {
        _service = service;
        _lang = lang;
        _jwtHelper = jwtHelper;
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
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetPosts([FromQuery] PaginationRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.GetPostsListAsync(request.Cursor, user.Id, request.PageSize);

        Console.WriteLine(res.Count());
        return ApiResponse.Success(
            new PaginationResponse
            {
                Items = res,
                Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                PageSize = request.PageSize,
            }
        );
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
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetMyPosts([FromQuery] PaginationRequest request)
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

    [HttpGet("username/{username}")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetPostsByUsername(
        string username,
        [FromQuery] PaginationRequest request
    )
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.GetPostsByUsername(
            username,
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
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetPostsByLink(string link)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.GetPostByLinkAsync(link, user.Id);

        return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound(_lang.Get("NoPost"));
    }

    /// <summary>
    /// Likes a specific post.
    /// </summary>
    /// <param name="id">The unique identifier of the post to like.</param>
    /// <returns>
    /// 200 - Returns success if post is liked successfully.
    /// 404 - Returns error if post does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("{id}/like")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> LikePost(Guid id)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.LikePostAsync(id, user.Id);

        return res ? ApiResponse.Success() : ApiResponse.NotFound(_lang.Get("NoPost"));
    }

    /// <summary>
    /// Removes a like from a specific post.
    /// </summary>
    /// <param name="id">The unique identifier of the post to unlike.</param>
    /// <returns>
    /// 200 - Returns success if like is removed successfully.
    /// 404 - Returns error if post does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpDelete("{id}/cancel-like")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> CancelLikePost(Guid id)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.CancelLikePostAsync(id, user.Id);

        return res ? ApiResponse.Success() : ApiResponse.NotFound(_lang.Get("NoPost"));
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
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetPostComments(
        Guid postId,
        [FromQuery] PaginationRequest request
    )
    {
        var user = _jwtHelper.GetAccountInfo();
        var res = await _service.GetPostCommentsList(
            postId,
            request.Cursor,
            user.Id,
            request.PageSize
        );

        return res != null
            ? ApiResponse.Success(
                new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = request.PageSize,
                }
            )
            : ApiResponse.NotFound(_lang.Get("NoPost"));
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
    [CheckStatusHelper(BusinessObject.Enums.StatusType.Active)]
    public async Task<IActionResult> AddPost([FromBody] CreatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ApiResponse.BadRequest(_lang.Get("PostEmpty"));
        }

        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.AddPostAsync(request, user.Id);

        return res == null
            ? ApiResponse.BadRequest(_lang.Get("NoAccount"))
            : ApiResponse.Created(res, _lang.Get("PostCreated"));
    }

    [HttpPut("{id}")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> UpdatePost(Guid id, [FromForm] UpdatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ApiResponse.BadRequest(_lang.Get("PostEmpty"));
        }

        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.UpdatePostAsync(request, id, user.Id);

        return res == null
            ? ApiResponse.NotFound(_lang.Get("NoPost"))
            : ApiResponse.Success(res, _lang.Get("PostUpdated"));
    }

    [HttpDelete("{id}")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.DeletePostAsync(id, user.Id);

        return res
            ? ApiResponse.Success(message: _lang.Get("PostDeleted"))
            : ApiResponse.NotFound(_lang.Get("NoPost"));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPostImage([FromForm] UploadPostImageRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.UploadPostImagesAsync(request, user.Id);

        return ApiResponse.Success(res);
    }
}
