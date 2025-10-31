using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Dtos;
using WebAPI.Helpers;

namespace WebAPI.Controller
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService _authService = null!;
        private readonly JwtHelper _jwtHelper;

        public AuthController(IAuthService authService, JwtHelper jwtHelper)

        {
            _authService = authService;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return ApiResponse.BadRequest("Username and password are required.");
                }

                var authResponse = await _authService.GetAuthenticateAsync(request.Username, request.Password);

                return authResponse != null
                    ? ApiResponse.Success(authResponse)
                    : ApiResponse.Unauthorized();
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) ||
                    string.IsNullOrEmpty(request.Password) ||
                    string.IsNullOrEmpty(request.Email))
                {
                    return ApiResponse.BadRequest("Username, password, and email are required.");
                }

                var res = await _authService.RegisterAccountAsync(request);

                if (res != null)
                {

                    return ApiResponse.Success("Registration successful. Please check your email to confirm your account.");
                }

                return ApiResponse.BadRequest("Username already exists.");

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmAccount([FromQuery] string token)
        {
            try
            {
                var res = await _authService.ConfirmAccountAsync(token);

                return res
                    ? ApiResponse.Success("Email confirmed successfully. You can now log in.")
                    : ApiResponse.BadRequest("Invalid or expired confirmation token.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string token)
        {
            try
            {
                var authResponse = await _authService.GetRefreshTokenAsync(token);

                return authResponse != null
                    ? ApiResponse.Success(authResponse)
                    : ApiResponse.Unauthorized();
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var ok = await _authService.RemoveRefresh(user.Id);

                return ok ? ApiResponse.Success() : ApiResponse.Unauthorized();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}
