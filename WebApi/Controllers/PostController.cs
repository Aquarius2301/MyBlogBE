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

        [HttpGet("link/{link}")]
        public async Task<IActionResult> GetPostsByLink(string link)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.GetPostByLinkAsync(link, user.Id);

                return ApiResponse.Success(res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(Guid id)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.LikePostAsync(id, user.Id);

                return ApiResponse.Success(res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("{id}/cancel-like")]
        public async Task<IActionResult> CancelLikePost(Guid id)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.CancelLikePostAsync(id, user.Id);

                return ApiResponse.Success(res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

    }
}
