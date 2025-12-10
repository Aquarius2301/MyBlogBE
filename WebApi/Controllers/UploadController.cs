using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _service;

        public UploadController(IUploadService uploadService)
        {
            _service = uploadService;
        }

        [HttpPost("")]
        public async Task<IActionResult> UploadPicture([FromForm] UploadRequest request)
        {
            // Placeholder implementation for file upload
            if (request == null || request.Pictures.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var res = await _service.UploadFiles(request.Pictures);

            return ApiResponse.Success(res);
        }
    }
}
