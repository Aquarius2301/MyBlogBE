using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly JwtHelper _jwtHelper;
    private readonly BaseSettings _settings;

    public AccountController(
        IAccountService service,
        JwtHelper jwtHelper,
        IOptions<BaseSettings> options
    )
    {
        _service = service;
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

        var isOldPasswordCorrect = await _service.IsPasswordCorrectAsync(
            user.Id,
            request.OldPassword
        );

        if (!isOldPasswordCorrect)
        {
            return ApiResponse.BadRequest("Old password is incorrect.");
        }

        if (request.NewPassword != request.NewPasswordConfimation)
        {
            return ApiResponse.BadRequest("New password and confirmation do not match.");
        }

        var result = await _service.ChangePasswordAsync(user.Id, request);

        return ApiResponse.Success("Password changed successfully.");
    }
}
