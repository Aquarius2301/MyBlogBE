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
    public async Task<IActionResult> GetChildComments(
        Guid id,
        [FromQuery] PaginationRequest request
    )
    {
        try
        {
            request.ApplyDefaults(_settings);

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
}
