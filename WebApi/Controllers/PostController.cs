using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services.Interfaces;
using WebAPI.Dtos;
using WebAPI.Helpers;

namespace WebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly JwtHelper _jwtHelper;

        public PostController(IPostService postService, JwtHelper jwtHelper)
        {
            _postService = postService;
            _jwtHelper = jwtHelper;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPosts([FromQuery] DateTime? cursor, int pageSize = 5)
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var res = await _postService.GetPostsListAsync(cursor, user.Id, pageSize);

                return ApiResponse.Success(new PaginationResponse
                {
                    Items = res,
                    Cursor = res.LastOrDefault() != null ? res.Last().CreatedAt.ToString("O") : "",
                    PageSize = pageSize
                }
                );
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }
    }
}
