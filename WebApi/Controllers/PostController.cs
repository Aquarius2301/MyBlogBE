using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Helpers;
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
        private readonly CloudinaryHelper _cloudinaryHelper;

        public PostController(IPostService postService, JwtHelper jwtHelper, CloudinaryHelper cloudinaryHelper)
        {
            _postService = postService;
            _jwtHelper = jwtHelper;
            _cloudinaryHelper = cloudinaryHelper;
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

        [HttpPost("")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ABC([FromForm] ABCRequest file)
        {
            try
            {
                var res = _cloudinaryHelper.Upload(file.File);

                return ApiResponse.Success(res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpDelete("")]
        public async Task<IActionResult> ABC([FromQuery] string id)
        {
            try
            {
                var res = _cloudinaryHelper.Delete(id);

                return ApiResponse.Success(res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }
    }

}
