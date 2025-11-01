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

                if (res.Item1)
                {
                    return ApiResponse.Success("Registration successful. Please check your email to confirm your account.");
                }
                else if (res.Item2 == "user")
                {
                    return ApiResponse.BadRequest("Username already exists.");
                }

                return ApiResponse.BadRequest("Email already exists.");

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmAccount([FromQuery] string type, [FromQuery] string token)
        {
            try
            {
                if (type == "register")
                {
                    var res = await _authService.ConfirmRegisterAccountAsync(token);
                    return res
                        ? ApiResponse.Success("Email confirmed successfully.")
                        : ApiResponse.BadRequest("Invalid or expired confirmation token.");
                }
                else if (type == "forgetPassword")
                {
                    var res = await _authService.ConfirmForgetPasswordAccountAsync(token);

                    return !string.IsNullOrEmpty(res)
                       ? ApiResponse.Success(res)
                       : ApiResponse.BadRequest("Invalid or expired confirmation token.");
                }
                else
                {
                    return ApiResponse.BadRequest("Type must be \"register\" or \"forgetPassword\"");
                }

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var authResponse = await _authService.GetRefreshTokenAsync(request.Token);

                return authResponse != null
                    ? ApiResponse.Success(authResponse)
                    : ApiResponse.Unauthorized();
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string identifier)
        {
            try
            {
                var res = await _authService.ForgotPasswordAsync(identifier);

                return res
                  ? ApiResponse.Success("Please check your email to confirm your account.")
                  : ApiResponse.NotFound("Not found the account");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromQuery] string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    ApiResponse.BadRequest("Password is required");
                }

                var res = await _authService.ResetPasswordAsync(token, password);

                return res
                    ? ApiResponse.Success("Password changed successfully.")
                    : ApiResponse.BadRequest("Invalid or expired confirmation token.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = _jwtHelper.GetAccountInfo();

                var ok = await _authService.RemoveRefresh(user.Id);

                return ok ? ApiResponse.Success("Logged out successfully") : ApiResponse.Unauthorized();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}
