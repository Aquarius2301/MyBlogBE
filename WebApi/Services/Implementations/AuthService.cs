using BusinessObject.Enums;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Settings;

namespace WebApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtHelper _jwtHelper;
    private readonly EmailHelper _emailHelper;
    private readonly BaseSettings _settings;

    public AuthService(
        IUnitOfWork unitOfWork,
        JwtHelper jwtHelper,
        EmailHelper emailHelper,
        IOptions<BaseSettings> options
    )
    {
        _unitOfWork = unitOfWork;
        _jwtHelper = jwtHelper;
        _emailHelper = emailHelper;
        _settings = options.Value;
    }

    public Task<Account?> GetByUsernameAsync(string username)
    {
        return _unitOfWork.Accounts.GetByUsernameAsync(username, includeInactive: true); // Avoid create 2 accounts with same username
    }

    public Task<Account?> GetByEmailAsync(string email)
    {
        return _unitOfWork.Accounts.GetByEmailAsync(email, includeInactive: true); // Avoid create 2 accounts with same email
    }

    public async Task<AuthResponse?> GetAuthenticateAsync(string username, string password)
    {
        var account = await _unitOfWork.Accounts.GetByUsernameAsync(username);

        if (
            account == null
            || account.Status == StatusType.InActive
            || !PasswordHasherHelper.VerifyPassword(password, account.HashedPassword)
        )
        {
            return null;
        }

        account.AccessToken = _jwtHelper.GenerateAccessToken(account);
        account.RefreshToken = _jwtHelper.GenerateRefreshToken();
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            _settings.JwtSettings.RefreshTokenDurationDays
        );
        account.SelfRemoveTime = null; // cancel self-removal if user logs in again

        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = account.AccessToken,
            RefreshToken = account.RefreshToken,
        };
    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(string refreshToken)
    {
        var account = await _unitOfWork.Accounts.GetByRefreshTokenAsync(refreshToken);

        if (account != null && account.RefreshToken == refreshToken)
        {
            if (account.RefreshTokenExpiryTime >= DateTime.UtcNow)
            {
                account.AccessToken = _jwtHelper.GenerateAccessToken(account);
                // account.RefreshToken = _jwtHelper.GenerateRefreshToken();

                await _unitOfWork.SaveChangesAsync();

                return new AuthResponse
                {
                    AccessToken = account.AccessToken,
                    RefreshToken = account.RefreshToken,
                };
            }
            else
            {
                account.AccessToken = null;
                account.RefreshToken = null;
                account.RefreshTokenExpiryTime = null;

                await _unitOfWork.SaveChangesAsync();

                return null;
            }
        }

        return null;
    }

    public async Task<bool> RemoveRefresh(Guid accountId)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

        if (account != null)
        {
            account.AccessToken = null;
            account.RefreshToken = null;
            account.RefreshTokenExpiryTime = null;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        return false;
    }

    public async Task<RegisterResponse> RegisterAccountAsync(RegisterRequest request)
    {
        var tokenLength = _settings.TokenLength;
        var tokenTimeout = _settings.TokenExpiryMinutes;
        var confirmCode = StringHelper.GenerateRandomString(tokenLength, true);
        var currentTime = DateTime.UtcNow;

        await _emailHelper.SendRegisterEmailAsync(request.Email, request.Username, confirmCode);

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            EmailVerifiedCode = confirmCode,
            VerificationType = VerificationType.Register,
            EmailVerifiedCodeExpiry = currentTime.AddMinutes(tokenTimeout),
            HashedPassword = PasswordHasherHelper.HashPassword(request.Password),
            Status = StatusType.InActive,
            CreatedAt = currentTime,
        };

        _unitOfWork.Accounts.Add(newAccount);
        await _unitOfWork.SaveChangesAsync();

        return new RegisterResponse
        {
            Id = newAccount.Id,
            Username = newAccount.Username,
            DisplayName = newAccount.DisplayName,
            DateOfBirth = newAccount.DateOfBirth,
        };
    }

    public async Task<bool> ConfirmRegisterAccountAsync(string confirmCode)
    {
        var account = await _unitOfWork.Accounts.GetByEmailVerifiedCode(
            confirmCode,
            VerificationType.Register
        );

        if (account != null && DateTime.UtcNow <= account.EmailVerifiedCodeExpiry)
        {
            account.EmailVerifiedCode = null;
            account.VerificationType = null;
            account.EmailVerifiedCodeExpiry = null;
            account.Status = StatusType.Active;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<string?> ConfirmForgotPasswordAccountAsync(string confirmCode)
    {
        var account = await _unitOfWork.Accounts.GetByEmailVerifiedCode(
            confirmCode,
            VerificationType.ForgotPassword
        );

        if (account != null && DateTime.UtcNow <= account.EmailVerifiedCodeExpiry)
        {
            account.VerificationType = VerificationType.ChangePassword;
            account.EmailVerifiedCodeExpiry = null;

            await _unitOfWork.SaveChangesAsync();
            return confirmCode;
        }

        return null;
    }

    public async Task<bool> ForgotPasswordAsync(string identifier)
    {
        var tokenTimeout = _settings.TokenExpiryMinutes;
        var account = await _unitOfWork.Accounts.GetByUsernameOrEmailAsync(identifier);

        if (account != null)
        {
            var confirmCode = StringHelper.GenerateRandomString(_settings.TokenLength);
            await _emailHelper.SendForgotPasswordEmailAsync(
                account.Email,
                account.Username,
                confirmCode
            );

            account.EmailVerifiedCode = confirmCode;
            account.VerificationType = VerificationType.ForgotPassword;
            account.EmailVerifiedCodeExpiry = DateTime.UtcNow.AddMinutes(tokenTimeout);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> ResetPasswordAsync(string confirmCode, string newPassowrd)
    {
        var account = await _unitOfWork.Accounts.GetByEmailVerifiedCode(
            confirmCode,
            VerificationType.ChangePassword
        );

        if (account != null)
        {
            account.VerificationType = null;
            account.EmailVerifiedCodeExpiry = null;
            account.EmailVerifiedCode = null;
            account.HashedPassword = PasswordHasherHelper.HashPassword(newPassowrd);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        return false;
    }
}
