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

        return account == null ? ApiResponse.NotFound("NoAccount") : ApiResponse.Success(account);
    }

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var account = await _service.GetProfileByIdAsync(id);

        return account == null ? ApiResponse.NotFound("NoAccount") : ApiResponse.Success(account);
    }

    [HttpGet("profile/username/{username}")]
    public async Task<IActionResult> GetProfileByUsername(string username)
    {
        var user = _jwtHelper.GetAccountInfo();

        var account = await _service.GetProfileByUsernameAsync(username, user.Id);

        return account == null ? ApiResponse.NotFound("NoAccount") : ApiResponse.Success(account);
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAccountName(
        [FromQuery] string name,
        [FromQuery] PaginationRequest pagination
    )
    {
        var res = await _service.GetAccountByNameAsync(
            name,
            pagination.Cursor,
            pagination.PageSize
        );

        return ApiResponse.Success(
            new PaginationResponse
            {
                Items = res.Item1,
                Cursor = res.Item2,
                PageSize = pagination.PageSize,
            }
        );
    }

    [HttpPut("profile/me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateAccountRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.UpdateAccountAsync(user.Id, request);

        return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("NoAccount");
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
            errors["OldPassword"] = "OldPasswordIncorrect";
        }

        // Check if the new password is different from the old password
        if (request.OldPassword == request.NewPassword)
        {
            errors["NewPassword"] = "NewPasswordMustBeDifferent";
        }

        // Check if the new password is strong enough
        if (!ValidationHelper.IsStrongPassword(request.NewPassword))
        {
            errors["NewPassword"] = "PasswordRegister";
        }

        if (errors.Count > 0)
        {
            return ApiResponse.BadRequest("PasswordChangeFailed", data: errors);
        }

        var res = await _service.ChangePasswordAsync(user.Id, request.NewPassword);

        return res
            ? ApiResponse.Success("PasswordChanged")
            : ApiResponse.BadRequest("PasswordChangeFailed");
    }

    [HttpPut("profile/me/change-avatar")]
    public async Task<IActionResult> ChangeMyAvatar([FromBody] ChangeAvatarRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.ChangeAvatarAsync(user.Id, request.Picture);

        return res ? ApiResponse.Success() : ApiResponse.NotFound("AvatarChangeFailed");
    }

    [HttpPut("profile/me/self-remove")]
    public async Task<IActionResult> SelfRemoveMyAccount()
    {
        var user = _jwtHelper.GetAccountInfo();

        var res = await _service.SelfRemoveAccount(user.Id);

        // if (res != null)
        // {
        //     var content =
        //         $"{_lang.Get("SelfRemoveAccountEmail1")} {_settings.SelfRemoveDurationDays} {_lang.Get("SelfRemoveAccountEmail2")} ({res})";
        //     return ApiResponse.Success(content);
        // }

        return res != null ? ApiResponse.Success(res) : ApiResponse.NotFound("AccountNotFound");
    }
}
