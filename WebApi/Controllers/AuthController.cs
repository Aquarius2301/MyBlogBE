using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Settings;

namespace WebApi.Controller;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly ILanguageService _lang;
    private readonly JwtSettings _settings;
    private readonly JwtHelper _jwtHelper;

    public AuthController(
        IAuthService service,
        ILanguageService lang,
        IOptions<JwtSettings> settings,
        JwtHelper jwtHelper
    )
    {
        _service = service;
        _lang = lang;
        _settings = settings.Value;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="request">Login credentials containing username and password.</param>
    /// <returns>
    /// 200 - Returns authentication token if credentials are valid.
    /// 400 - Returns validation errors if input is invalid.
    /// 401 - Returns error if authentication fails.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        try
        {
            // Validate input
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                errors["Username"] = _lang.Get("UsernameEmpty");
            }
            if (string.IsNullOrEmpty(request.Password))
            {
                errors["Password"] = _lang.Get("PasswordEmpty");
            }
            if (errors.Any())
            {
                return ApiResponse.BadRequest(_lang.Get("LoginFailed"), errors);
            }

            // Authenticate user
            var authResponse = await _service.GetAuthenticateAsync(
                request.Username,
                request.Password
            );

            // Save cookie on server
            // var cookieOptions = new CookieOptions
            // {
            //     HttpOnly = true,
            //     Expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenDurationDays),
            //     Secure = true,
            //     SameSite = SameSiteMode.Lax,
            // };

            // var cookieOptions = new CookieOptions
            // {
            //     HttpOnly = true,
            //     Expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenDurationDays),
            //     Secure = true,
            //     SameSite = SameSiteMode.Lax,
            // };

            if (authResponse != null)
            {
                Response.Cookies.Append(
                    "accessToken",
                    authResponse.AccessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(
                            _settings.AccessTokenDurationMinutes
                        ),
                        Path = "/",
                    }
                );
                Response.Cookies.Append(
                    "refreshToken",
                    authResponse.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDurationDays),
                        Path = "/",
                    }
                );
                return ApiResponse.Success(authResponse, _lang.Get("LoginSuccess"));
            }
            return ApiResponse.Unauthorized(_lang.Get("LoginFailed"));

            // return authResponse != null
            //     ? ApiResponse.Success(authResponse)
            //     : ApiResponse.Unauthorized(_lang.Get("LoginFailed"));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration data including username, email, password, display name, and date of birth.</param>
    /// <returns>
    /// 200 - Returns success message and send email to confirm if registration is successful.
    /// 400 - Returns validation errors if input is invalid or user already exists.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var errors = string.Empty;

        // Check validation
        if (!ValidationHelper.IsValidString(request.Username, true, 3, 20))
            errors += _lang.Get("UsernameRegister") + "<br/>";

        if (!ValidationHelper.IsValidString(request.DisplayName, false, 3, 50))
            errors += _lang.Get("DisplaynameRegister") + "<br/>";

        if (!ValidationHelper.IsStrongPassword(request.Password))
            errors += _lang.Get("PasswordRegister") + "<br/>";

        if (!ValidationHelper.IsValidEmail(request.Email))
            errors += _lang.Get("EmailRegister") + "<br/>";

        if (
            !ValidationHelper.IsValidDateOfBirth(request.DateOfBirth.ToDateTime(new TimeOnly(0, 0)))
        )
            errors += _lang.Get("DobRegister") + "<br/>";
        if (!string.IsNullOrEmpty(errors))
        {
            return ApiResponse.BadRequest(errors.Trim());
        }

        // Check existence
        if (await _service.GetByUsernameAsync(request.Username) != null)
        {
            errors += _lang.Get("UsernameExist") + "<br/>";
        }
        if (await _service.GetByEmailAsync(request.Email) != null)
        {
            errors += _lang.Get("EmailExist") + "<br/>";
        }
        if (!string.IsNullOrEmpty(errors))
        {
            return ApiResponse.BadRequest(errors.Trim());
        }

        // Register account if no errors
        var res = await _service.RegisterAccountAsync(request);

        return ApiResponse.Success(res, _lang.Get("ConfirmEmail"));
    }

    /// <summary>
    /// Confirms user account registration or password reset via token.
    /// </summary>
    /// <param name="type">Type of confirmation: "register" or "forgotPassword".</param>
    /// <param name="token">Confirmation token sent to user's email.</param>
    /// <returns>
    /// 200 - Returns success message if token is valid.
    /// 400 - Returns error if type is invalid or token is invalid.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpGet("confirm")]
    // [CheckStatusHelper([BusinessObject.Enums.StatusType.InActive])]
    public async Task<IActionResult> ConfirmAccount(
        [FromQuery] string type,
        [FromQuery] string token
    )
    {
        if (type != "register" && type != "forgotPassword")
        {
            return ApiResponse.BadRequest(_lang.Get("TypeError"));
        }

        if (type == "register")
        {
            var res = await _service.ConfirmRegisterAccountAsync(token);
            return res
                ? ApiResponse.Success(res, _lang.Get("ConfirmSuccessful"))
                : ApiResponse.BadRequest(_lang.Get("InvalidToken"));
        }
        else
        {
            var res = await _service.ConfirmForgotPasswordAccountAsync(token);

            return !string.IsNullOrEmpty(res)
                ? ApiResponse.Success(res, _lang.Get("ForgotPasswordConfirmSuccessful"))
                : ApiResponse.BadRequest(_lang.Get("InvalidToken"));
        }
    }

    /// <summary>
    /// Refreshes authentication token using refresh token.
    /// </summary>
    /// <param name="request">Request containing refresh token.</param>
    /// <returns>
    /// 200 - Returns new authentication token if refresh token is valid.
    /// 401 - Returns error if refresh token is invalid or expired.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("refresh")]
    // [CheckStatusHelper([
    //     BusinessObject.Enums.StatusType.Active,
    //     BusinessObject.Enums.StatusType.Suspended,
    // ])]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ApiResponse.Unauthorized(_lang.Get("InvalidToken"));
        }
        var authResponse = await _service.GetRefreshTokenAsync(refreshToken);

        if (authResponse != null)
        {
            Response.Cookies.Append(
                "accessToken",
                authResponse.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDurationDays),
                    Path = "/",
                }
            );

            return ApiResponse.Success(authResponse);
        }

        return ApiResponse.Unauthorized(_lang.Get("InvalidToken"));
    }

    /// <summary>
    /// Initiates password reset process by sending confirmation email.
    /// </summary>
    /// <param name="request">Request containing username or email identifier.</param>
    /// <returns>
    /// 200 - Returns success message if account is found and email is sent.
    /// 400 - Returns error if account does not exist.
    /// 500 -  Returns error message if exception occurs.
    /// </returns>
    [HttpPost("forgot-password")]
    // [CheckStatusHelper([
    //     BusinessObject.Enums.StatusType.Active,
    //     BusinessObject.Enums.StatusType.Suspended,
    // ])]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordResponse request)
    {
        var res = await _service.ForgotPasswordAsync(request.Identifier);

        return res
            ? ApiResponse.Success(res, _lang.Get("ConfirmEmail"))
            : ApiResponse.BadRequest(_lang.Get("NoAccount"));
    }

    /// <summary>
    /// Resets user password using confirmation code.
    /// </summary>
    /// <param name="request">Request containing confirmation code and new password.</param>
    /// <returns>
    /// 200 - Returns success message if password is reset successfully.
    /// 400 - Returns error if confirmation code or password is invalid.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ConfirmCode))
        {
            return ApiResponse.BadRequest(_lang.Get("TokenEmpty"));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiResponse.BadRequest(_lang.Get("PasswordEmpty"));
        }

        var res = await _service.ResetPasswordAsync(request.ConfirmCode, request.NewPassword);

        return res
            ? ApiResponse.Success(res, _lang.Get("PasswordChanged"))
            : ApiResponse.BadRequest(_lang.Get("InvalidToken"));
    }

    /// <summary>
    /// Logs out the authenticated user by removing their refresh token.
    /// </summary>
    /// <returns>
    /// 200 - Returns success message if logout is successful.
    /// 401 - Returns error if user is not authenticated or logout fails.
    /// 500 - Returns error message if exception occurs.
    /// </returns>
    [Authorize]
    [HttpPost("logout")]
    [CheckStatusHelper([
        BusinessObject.Enums.StatusType.Active,
        BusinessObject.Enums.StatusType.Suspended,
    ])]
    public async Task<IActionResult> Logout()
    {
        var user = _jwtHelper.GetAccountInfo();

        var ok = await _service.RemoveRefresh(user.Id);

        Response.Cookies.Delete(
            "accessToken",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            }
        );

        Response.Cookies.Delete(
            "refreshToken",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            }
        );

        return ok ? ApiResponse.Success(_lang.Get("LoggedOut")) : ApiResponse.Unauthorized();
    }
}
