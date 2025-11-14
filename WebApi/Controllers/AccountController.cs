using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Ocsp;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Settings;

namespace WebApi.Controllers;

[Authorize]
[Route("api/account")]
[ApiController]
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

        if (account == null)
        {
            return ApiResponse.NotFound("Account not found.");
        }

        return ApiResponse.Success(account);
    }

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var account = await _service.GetProfileByIdAsync(id);

        if (account == null)
        {
            return ApiResponse.NotFound("Account not found.");
        }

        return ApiResponse.Success(account);
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
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var account = await _service.UpdateAccountAsync(user.Id, request);

            return ApiResponse.Success(account);
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    [HttpPut("profile/me/change-password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] UpdatePasswordRequest request)
    {
        var user = _jwtHelper.GetAccountInfo();

        var errors = new Dictionary<string, string>();

        if (
            string.IsNullOrWhiteSpace(request.OldPassword)
            || !await _service.IsPasswordCorrectAsync(user.Id, request.OldPassword)
        )
        {
            errors["OldPassword"] = _lang.Get("OldPasswordIncorrect");
        }

        if (ValidationHelper.IsStrongPassword(request.NewPassword) == false)
        {
            errors["NewPassword"] = _lang.Get("PasswordRegister");
        }

        if (request.OldPassword == request.NewPassword)
        {
            errors["NewPassword"] = _lang.Get("NewPasswordMustBeDifferent");
        }

        if (errors.Count > 0)
        {
            return ApiResponse.BadRequest(data: errors);
        }

        await _service.ChangePasswordAsync(user.Id, request);

        return ApiResponse.Success(_lang.Get("PasswordChanged"));
    }

    [HttpPut("profile/me/change-avatar")]
    public async Task<IActionResult> ChangeMyAvatar([FromForm] ChangeAvatarRequest request)
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            var imageDto = await _service.ChangeAvatarAsync(user.Id, request.AvatarFile);

            return ApiResponse.Success(imageDto);
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }

    [HttpPut("profile/me/self-remove")]
    public async Task<IActionResult> SelfRemoveMyAccount()
    {
        try
        {
            var user = _jwtHelper.GetAccountInfo();

            await _service.SelfRemoveAccount(user.Id);

            return ApiResponse.Success(
                $"{_lang.Get("SelfRemoveAccount1")} {_settings.SelfRemoveDurationDays}{_lang.Get("SelfRemoveAccount2")}"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse.Error(ex.Message);
        }
    }
}
