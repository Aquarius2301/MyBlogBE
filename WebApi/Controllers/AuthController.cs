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

        /// <summary>
        /// Authenticates the user and generates access and refresh tokens.
        /// </summary>
        /// <param name="request">
        /// The authentication request containing the username and password.
        /// </param>
        /// <returns>
        /// <para>200: Returns <see cref="AuthResponse"/> if authentication succeeds.</para>
        /// <para>400: If username or password is blank.</para>
        /// <para>401: If authentication fails.</para>
        /// </returns>

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

        /// <summary>
        /// Registers a new user account and sends a confirmation email.
        /// </summary>
        /// <param name="request">
        /// The registration request containing the username, password, email, and display name.
        /// </param>
        /// <returns>
        /// <para>200 – Registration successful. A confirmation email has been sent.</para>
        /// <para>400 – Returned if any required field is missing, or if the username/email already exists.</para>
        /// <para>500 – Returned if an unexpected server error occurs.</para>
        /// </returns>
        /// <remarks>
        /// This endpoint creates a new user, stores their credentials securely,  
        /// and sends a verification link to the provided email address.
        /// </remarks>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) ||
                    string.IsNullOrEmpty(request.Password) ||
                    string.IsNullOrEmpty(request.Email) ||
                    string.IsNullOrEmpty(request.DisplayName))
                {
                    return ApiResponse.BadRequest("Username, display name, password, and email are required.");
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

        /// <summary>
        /// Confirms a user action using a verification token.
        /// </summary>
        /// <param name="type">
        /// The type of confirmation.  
        /// Accepts either <c>"register"</c> for email verification  
        /// or <c>"forgotPassword"</c> for password reset confirmation.
        /// </param>
        /// <param name="token">The unique verification token sent to the user's email.</param>
        /// <returns>
        /// <para>200 – Confirmation successful (email verified or password reset confirmed).</para>
        /// <para>400 – Invalid, missing, or expired token, or incorrect type value.</para>
        /// <para>500 – Unexpected server error.</para>
        /// </returns>
        /// <remarks>
        /// Depending on the <c>type</c> parameter, this endpoint verifies either  
        /// a new registration or a password reset request.
        /// </remarks>
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
                else if (type == "forgotPassword")
                {
                    var res = await _authService.ConfirmForgotPasswordAccountAsync(token);

                    return !string.IsNullOrEmpty(res)
                       ? ApiResponse.Success(res)
                       : ApiResponse.BadRequest("Invalid or expired confirmation token.");
                }
                else
                {
                    return ApiResponse.BadRequest("Type must be register or forgotPassword");
                }

            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Refreshes the access token using a valid refresh token.
        /// </summary>
        /// <param name="request">
        /// The request object containing the refresh token.
        /// </param>
        /// <returns>
        /// <para>200 – Successfully generated a new access token.</para>
        /// <para>401 – Invalid or expired refresh token.</para>
        /// <para>500 – Server error while refreshing token.</para>
        /// </returns>
        /// <remarks>
        /// This endpoint should be called when the access token expires.  
        /// The refresh token must be valid and unexpired.
        /// </remarks>

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

        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        /// <param name="request">
        /// Contains the account identifier (username or email) of the user requesting password reset.
        /// </param>
        /// <returns>
        /// <para>200 – Email sent successfully with reset instructions.</para>
        /// <para>404 – Account not found.</para>
        /// <para>500 – Failed to process password reset request.</para>
        /// </returns>
        /// <remarks>
        /// This endpoint generates a password reset token and sends it  
        /// to the user’s registered email address for verification.
        /// </remarks>

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordResponse request)
        {
            try
            {
                var res = await _authService.ForgotPasswordAsync(request.Identifier);

                return res
                  ? ApiResponse.Success("Please check your email to confirm your account.")
                  : ApiResponse.NotFound("Not found the account");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Resets the user's password using a confirmation token.
        /// </summary>
        /// <param name="request">
        /// The reset request containing the confirmation token and the new password.
        /// </param>
        /// <returns>
        /// <para>200 – Password reset successfully.</para>
        /// <para>400 – Invalid, missing, or expired confirmation token.</para>
        /// <para>500 – Error occurred while updating password.</para>
        /// </returns>
        /// <remarks>
        /// The <c>ConfirmCode</c> is obtained from the password reset email.  
        /// Both <c>ConfirmCode</c> and <c>NewPassword</c> are required.
        /// </remarks>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ConfirmCode))
                {
                    return ApiResponse.BadRequest("Confirmation token is required");
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return ApiResponse.BadRequest("Password is required");
                }

                var res = await _authService.ResetPasswordAsync(request.ConfirmCode, request.NewPassword);

                return res
                    ? ApiResponse.Success("Password changed successfully.")
                    : ApiResponse.BadRequest("Invalid or expired confirmation token.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Logs the current user out and invalidates their refresh token.
        /// </summary>
        /// <returns>
        /// <para>200 – Logged out successfully.</para>
        /// <para>401 – Unauthorized or session already invalid.</para>
        /// <para>500 – Unexpected error while logging out.</para>
        /// </returns>
        /// <remarks>
        /// This endpoint requires authentication.  
        /// It removes the user’s refresh token from the database,  
        /// effectively logging them out from all active sessions.
        /// </remarks>
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
