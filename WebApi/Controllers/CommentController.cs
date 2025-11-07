using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services.Interfaces;
using WebAPI.Dtos;
using WebAPI.Helpers;

namespace WebApi.Controllers;

[Authorize]
[Route("api/comments")]
[ApiController]
public class CommentController : ControllerBase
{
    private ICommentService _commentService;
    private readonly JwtHelper _jwtHelper;

    public CommentController(ICommentService commentService, JwtHelper jwtHelper)
    {
        _commentService = commentService;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Get a paginated list of comments for a specific post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="cursor">Timestamp of the last loaded child comment (used for pagination).</param>
    /// <param name="pageSize">Number of comments per page.</param>
    /// <returns>
    /// <para>200: Returns a list of comments for the given post.</para>
    /// <para>404: If the post does not exist.</para>
    /// <para>500: If an unexpected error occurs.</para>
    /// </returns>
    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetComments(Guid postId, [FromQuery] DateTime? cursor, [FromQuery] int pageSize = 10)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();
            var res = await _commentService.GetCommentList(postId, cursor, user.Id, pageSize);

            return res != null ? ApiResponse.Success(new PaginationResponse
            {
                Items = res,
                Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                PageSize = pageSize
            })
            : ApiResponse.NotFound("Post not found");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Get a paginated list of child comments for a specific comment.
    /// </summary>
    /// <param name="id">The ID of the parent comment.</param>
    /// <param name="cursor">Timestamp of the last loaded child comment (used for pagination).</param>
    /// <param name="pageSize">Number of child comments per page.</param>
    /// <returns>
    /// <para>200: Returns a list of child comments.</para>
    /// <para>404: If the parent comment does not exist.</para>
    /// <para>500: If an unexpected error occurs.</para>
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChildComments(Guid id, [FromQuery] DateTime? cursor, [FromQuery] int pageSize = 10)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();
            var res = await _commentService.GetChildCommentList(id, cursor, user.Id, pageSize);

            return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("Comment not found");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Like a comment.
    /// </summary>
    /// <param name="id">The ID of the comment to like.</param>
    /// <returns>
    /// <para>200: Returns true if the like succeeds.</para>
    /// <para>400: If the comment does not exist.</para>
    /// <para>500: If an unexpected error occurs.</para>
    /// </returns>
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeComment(Guid id)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();
            var res = await _commentService.LikeCommentAsync(id, user.Id);

            return res ? ApiResponse.Success(res) : ApiResponse.NotFound("Comment not found");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Cancel a previously liked comment.
    /// </summary>
    /// <param name="id">The ID of the comment to cancel like.</param>
    /// <returns>
    /// <para>200: Returns true if the cancel like succeeds.</para>
    /// <para>400: If the comment does not exist.</para>
    /// <para>500: If an unexpected error occurs.</para>
    /// </returns>
    [HttpDelete("{id}/cancel-like")]
    public async Task<IActionResult> CancelLikeComment(Guid id)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();
            var res = await _commentService.CancelLikeCommentAsync(id, user.Id);

            return res ? ApiResponse.Success(res) : ApiResponse.NotFound("Comment not found");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }
}

