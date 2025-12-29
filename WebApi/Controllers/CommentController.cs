using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[Route("api/comments")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _service;
    private readonly ILanguageService _lang;
    private readonly JwtHelper _jwtHelper;

    public CommentController(ICommentService service, ILanguageService lang, JwtHelper jwtHelper)
    {
        _service = service;
        _lang = lang;
        _jwtHelper = jwtHelper;
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
        var user = _jwtHelper.GetAccountInfo();
        var res = await _service.GetChildCommentList(id, request.Cursor, user.Id, request.PageSize);

        return res.Item1 != null
            ? ApiResponse.Success(
                new PaginationResponse
                {
                    Items = res.Item1,
                    Cursor = res.Item2,
                    PageSize = request.PageSize,
                }
            )
            : ApiResponse.NotFound("NoComment");
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
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.LikeCommentAsync(id, user.Id);

        return res.HasValue ? ApiResponse.Success(res.Value) : ApiResponse.NotFound("NoComment");
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
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.CancelLikeCommentAsync(id, user.Id);

        return res.HasValue ? ApiResponse.Success(res.Value) : ApiResponse.NotFound("NoComment");
    }

    /// <summary>
    /// Adds a new comment.
    /// </summary>
    /// <param name="request">The comment creation request containing content and optional images.</param>
    /// <returns>
    /// 200 - Returns the created comment details.
    /// 400 - Returns error if request is invalid.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        // If ParentCommentId is provided, ReplyAccountId must also be provided, and vice versa
        if (request.ParentCommentId != null && request.ReplyAccountId == null)
            return ApiResponse.BadRequest("ReplyAccountRequired");

        if (request.ParentCommentId == null && request.ReplyAccountId != null)
            return ApiResponse.BadRequest("ParentCommentRequired");

        if (!ValidationHelper.IsValidString(request.Content))
        {
            return ApiResponse.BadRequest(_lang.Get("InvalidContent"));
        }

        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.AddCommentAsync(user.Id, request);

        return res != null
            ? ApiResponse.Success(res)
            : ApiResponse.BadRequest(_lang.Get("AddCommentFailed"));
    }

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to update.</param>
    /// <param name="request">The comment update request containing new content and optional images.</param>
    /// <returns>
    /// 200 - Returns the updated comment details.
    /// 400 - Returns error if request is invalid.
    /// 404 - Returns error if comment does not exist.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPut("{id}")]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> UpdateComment(Guid id, [FromForm] UpdateCommentRequest request)
    {
        if (!ValidationHelper.IsValidString(request.Content))
        {
            return ApiResponse.BadRequest(_lang.Get("InvalidContent"));
        }

        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.UpdateCommentAsync(id, request, user.Id);

        return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("NoComment");
    }

    /// <summary>
    /// Deletes a specific comment.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to delete.</param>
    /// <returns>
    /// 200 - Returns success message if deletion is successful.
    /// 404 - Returns error if comment is not found.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpDelete("{id}")]
    [CheckStatusHelper([BusinessObject.Enums.StatusType.Active])]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.DeleteCommentAsync(id, user.Id);

        return res ? ApiResponse.Success("CommentDeleted") : ApiResponse.NotFound("NoComment");
    }
}
