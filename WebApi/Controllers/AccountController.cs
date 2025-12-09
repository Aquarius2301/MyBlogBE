using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Settings;

namespace WebApi.Controllers;

[Authorize]
[Route("api/accounts")]
[ApiController]
[CheckStatusHelper([
    BusinessObject.Enums.StatusType.Active,
    BusinessObject.Enums.StatusType.Suspended,
])]
public class AccountController : ControllerBase
{
    private readonly IAccountService _service;
    private readonly ILanguageService _lang;
    private readonly JwtHelper _jwtHelper;
    private readonly BaseSettings _settings;

    public AccountController(
        IAccountService service,
        ILanguageService lang,
        JwtHelper jwtHelper,
        IOptions<BaseSettings> options
    )
    {
        _service = service;
        _lang = lang;
        _jwtHelper = jwtHelper;
        _settings = options.Value;
    }

    [HttpGet("profile/me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = _jwtHelper.GetAccountInfo();

        var account = await _service.GetProfileByIdAsync(user.Id);

        return account == null
            ? ApiResponse.NotFound("Account not found.")
            : ApiResponse.Success(account);
    }

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var account = await _service.GetProfileByIdAsync(id);

        return account == null
            ? ApiResponse.NotFound("Account not found.")
            : ApiResponse.Success(account);
    }

    [HttpGet("profile/username/{username}")]
    public async Task<IActionResult> GetProfileByUsername(string username)
    {
        var user = _jwtHelper.GetAccountInfo();

        var account = await _service.GetProfileByUsernameAsync(username, user.Id);

        return account == null
            ? ApiResponse.NotFound("Account not found.")
            : ApiResponse.Success(account);
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAccountName(
        [FromQuery] string name,
        [FromQuery] PaginationRequest pagination
    )
    {
        var account = await _service.GetAccountByNameAsync(
            name,
            pagination.Cursor,
            pagination.PageSize
        );

        return ApiResponse.Success(
            new PaginationResponse
            {
                Items = account,
                Cursor = account.Count > 0 ? account.Last().CreatedAt : null,
                PageSize = pagination.PageSize,
            }
        );
    }

    [HttpPut("profile/me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateAccountRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var account = await _service.UpdateAccountAsync(user.Id, request);

        return ApiResponse.Success(account);
    }

    [HttpPut("profile/me/change-password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] UpdatePasswordRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var errors = new Dictionary<string, string>();

        // Check if the old password is correct
        if (
            string.IsNullOrWhiteSpace(request.OldPassword) // empty check
            || !await _service.IsPasswordCorrectAsync(user.Id, request.OldPassword) // correct check
        )
        {
            errors["OldPassword"] = _lang.Get("OldPasswordIncorrect");
        }

        // Check if the new password is different from the old password
        if (request.OldPassword == request.NewPassword)
        {
            errors["NewPassword"] = _lang.Get("NewPasswordMustBeDifferent");
        }

        // Check if the new password is strong enough
        if (ValidationHelper.IsStrongPassword(request.NewPassword) == false)
        {
            errors["NewPassword"] = _lang.Get("PasswordRegister");
        }

        if (errors.Count > 0)
        {
            return ApiResponse.BadRequest(data: errors);
        }

        var res = await _service.ChangePasswordAsync(user.Id, request.NewPassword);

        return res
            ? ApiResponse.Success(_lang.Get("PasswordChanged"))
            : ApiResponse.BadRequest(_lang.Get("PasswordChangeFailed"));
    }

    [HttpPut("profile/me/change-avatar")]
    public async Task<IActionResult> ChangeMyAvatar([FromForm] ChangeAvatarRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var imageDto = await _service.ChangeAvatarAsync(user.Id, request.Avatar);

        return imageDto != null
            ? ApiResponse.Success(imageDto)
            : ApiResponse.NotFound(_lang.Get("AvatarChangeFailed"));
    }

    [HttpPut("profile/me/self-remove")]
    public async Task<IActionResult> SelfRemoveMyAccount()
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.SelfRemoveAccount(user.Id);

        if (res != null)
        {
            var content =
                $"{_lang.Get("SelfRemoveAccountEmail1")} {_settings.SelfRemoveDurationDays} {_lang.Get("SelfRemoveAccountEmail2")} ({res})";
            return ApiResponse.Success(content);
        }

        return ApiResponse.NotFound(_lang.Get("AccountNotFound"));
    }
}
