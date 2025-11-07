using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services.Interfaces;
using WebAPI.Dtos;
using WebAPI.Helpers;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly JwtHelper _jwtHelper;
        private readonly CloudinaryHelper _cloudinaryHelper;

        public PostController(IPostService postService, JwtHelper jwtHelper, CloudinaryHelper cloudinaryHelper)
        {
            _postService = postService;
            _jwtHelper = jwtHelper;
            _cloudinaryHelper = cloudinaryHelper;
        }

        /// <summary>
        /// Get a paginated list of posts for the home feed.
        /// </summary>
        /// <param name="cursor">Timestamp of the last loaded post (used for pagination).</param>
        /// <param name="pageSize">Number of posts to return per request.</param>
        /// <returns>
        /// <para>200: Returns a <see cref="PaginationResponse"/> containing a list of posts.</para>
        /// <para>500: If an unexpected error occurs.</para>
        /// </returns>
        [HttpGet("")]
        public async Task<IActionResult> GetPosts([FromQuery] DateTime? cursor, int pageSize = 10)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.GetPostsListAsync(cursor, user.Id, pageSize);

                return ApiResponse.Success(new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Get a paginated list of posts created by the current user.
        /// </summary>
        /// <param name="cursor">Timestamp of the last loaded post (used for pagination).</param>
        /// <param name="pageSize">Number of posts to return per request.</param>
        /// <returns>
        /// <para>200: Returns a <see cref="PaginationResponse"/> containing the user's posts.</para>
        /// <para>500: If an unexpected error occurs.</para>
        /// </returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPosts([FromQuery] DateTime? cursor, int pageSize = 10)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.GetMyPostsListAsync(cursor, user.Id, pageSize);

                return ApiResponse.Success(new PaginationResponse
                {
                    Items = res,
                    Cursor = res.Count() > 0 ? res.Last().CreatedAt : null,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Get the details of a post by its link (slug).
        /// </summary>
        /// <param name="link">The slug of the post.</param>
        /// <returns>
        /// <para>200: Returns a detailed post object.</para>
        /// <para>404: If the post is not found.</para>
        /// <para>500: If an unexpected error occurs.</para>
        /// </returns>
        [HttpGet("link/{link}")]
        public async Task<IActionResult> GetPostsByLink(string link)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.GetPostByLinkAsync(link, user.Id);

                return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("Post not found");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Like a post.
        /// </summary>
        /// <param name="id">The ID of the post to like.</param>
        /// <returns>
        /// <para>200: Returns true if the like is successful.</para>
        /// <para>200: Returns false if the post has already been liked by the user.</para>
        /// <para>400: If the post does not exist.</para>
        /// <para>500: If an unexpected error occurs.</para>
        /// </returns>
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(Guid id)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.LikePostAsync(id, user.Id);

                return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("Post not found");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Cancel a previously liked post.
        /// </summary>
        /// <param name="id">The ID of the post to cancel like.</param>
        /// <returns>
        /// <para>200: Returns true if the cancel like is successful.</para>
        /// <para>200: Returns false if the post was not liked by the user.</para>
        /// <para>400: If the post does not exist.</para>
        /// <para>500: If an unexpected error occurs.</para>
        /// </returns>
        [HttpDelete("{id}/cancel-like")]
        public async Task<IActionResult> CancelLikePost(Guid id)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.CancelLikePostAsync(id, user.Id);

                return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("Post not found");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

    }
}
