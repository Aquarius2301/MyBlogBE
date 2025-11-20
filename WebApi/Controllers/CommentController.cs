using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Settings;

namespace WebApi.Controllers;

[Authorize]
[Route("api/comments")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _service;
    private readonly ILanguageService _lang;
    private readonly JwtHelper _jwtHelper;
    private readonly BaseSettings _settings;

    public CommentController(
        ICommentService service,
        ILanguageService lang,
        JwtHelper jwtHelper,
        IOptions<BaseSettings> options
    )
    {
        _service = service;
        _lang = lang;
        _jwtHelper = jwtHelper;
        _settings = options.Value;
    }

    /// <summary>
    /// Retrieves a paginated list of child comments (replies) for a specific parent comment.
    /// </summary>
    /// <param name="id">The unique identifier of the parent comment.</param>
    /// <param name="request">Pagination parameters including cursor and page size.</param>
    /// <returns>
    /// 200 - Returns paginated list of child comments.
    /// 404 - Returns error if parent comment does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("{id}")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> GetChildComments(
        Guid id,
        [FromQuery] PaginationRequest request
    )
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();
            var res = await _service.GetChildCommentList(
                id,
                request.Cursor,
                user.Id,
                request.PageSize
            );

            return res != null
                ? ApiResponse.Success(res)
                : ApiResponse.NotFound(_lang.Get("NoComment"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Likes a specific comment.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to like.</param>
    /// <returns>
    /// 200 - Returns success if comment is liked successfully.
    /// 404 - Returns error if comment does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("{id}/like")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> LikeComment(Guid id)
    {
        try
        {
            // Validate input
            if (await _service.GetByIdAsync(id) == null)
            {
                return ApiResponse.NotFound(_lang.Get("NoComment"));
            }

            var user = _jwtHelper.GetAccountInfo();
            var res = await _service.LikeCommentAsync(id, user.Id);

            return res ? ApiResponse.Success(res) : ApiResponse.NotFound(_lang.Get("NoComment"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Removes a like from a specific comment.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to unlike.</param>
    /// <returns>
    /// 200 - Returns success if like is removed successfully.
    /// 404 - Returns error if comment does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpDelete("{id}/cancel-like")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> CancelLikeComment(Guid id)
    {
        try
        {
            // Validate input
            if (await _service.GetByIdAsync(id) == null)
            {
                return ApiResponse.NotFound(_lang.Get("NoComment"));
            }

            var user = _jwtHelper.GetAccountInfo();
            var res = await _service.CancelLikeCommentAsync(id, user.Id);

            return res ? ApiResponse.Success(res) : ApiResponse.NotFound(_lang.Get("NoComment"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    [HttpPost]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> AddComment([FromForm] CreateCommentRequest request)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            if (request.ParentCommentId != null)
            {
                var parentComment = await _service.GetByIdAsync(request.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return ApiResponse.NotFound(_lang.Get("NoParentComment"));
                }
                if (request.ReplyAccountId == null) // if there is a parent comment, but no reply account id
                {
                    return ApiResponse.BadRequest(_lang.Get("NoReplyAccount"));
                }
            }
            else if (request.ReplyAccountId != null)
            {
                // if there is no parent comment, but reply account id is provided
                return ApiResponse.BadRequest(_lang.Get("NoParentComment"));
            }

            var pictures = new List<ImageDto>();

            if (request.Images != null && request.Images.Count > 0)
            {
                pictures = await _service.AddImageAsync(request.Images);
            }

            var res = await _service.AddCommentAsync(user.Id, request, pictures);

            return ApiResponse.Success(res);
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> UpdateComment(Guid id, [FromForm] UpdateCommentRequest request)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var existingComment = await _service.GetByIdAsync(id);
            if (existingComment == null || existingComment.AccountId != user.Id)
            {
                return ApiResponse.NotFound(_lang.Get("NoComment"));
            }

            var pictures = new List<ImageDto>();

            if (request.ClearImages)
            {
                Console.WriteLine("Updating images...");
                pictures = await _service.UpdateImageAsync(id, request.Images ?? []);
            }
            else
                pictures = null;

            var res = await _service.UpdateCommentAsync(id, request, pictures);
            return res != null
                ? ApiResponse.Success(res)
                : ApiResponse.NotFound(_lang.Get("NoComment"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.ToString());
        }
    }

    [HttpDelete("{id}")]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var existingComment = await _service.GetByIdAsync(id);
            if (existingComment == null || existingComment.AccountId != user.Id)
            {
                return ApiResponse.NotFound(_lang.Get("NoComment"));
            }

            await _service.DeleteImageAsync(id);

            var res = await _service.DeleteCommentAsync(id);

            return res ? ApiResponse.Success() : ApiResponse.NotFound(_lang.Get("NoComment"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }
}
