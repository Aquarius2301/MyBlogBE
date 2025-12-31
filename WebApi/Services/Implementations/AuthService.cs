using BusinessObject.Enums;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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

    public Task<Account?> GetAccountByNameOrEmailAsync(string identifier)
    {
        var query = _unitOfWork.Accounts.GetQuery();

        query = query.Where(a =>
            (a.Username == identifier || a.Email == identifier) && a.DeletedAt == null
        );

        return query.FirstOrDefaultAsync();
    }

    public Task<Account?> GetAccountByUsernameAsync(string username)
    {
        var query = _unitOfWork.Accounts.ReadOnly();

        query = query.Where(a => a.Username == username && a.DeletedAt == null);

        return query.FirstOrDefaultAsync();
    }

    public Task<Account?> GetAccountByEmailAsync(string email)
    {
        var query = _unitOfWork.Accounts.ReadOnly();

        query = query.Where(a => a.Email == email && a.DeletedAt == null);

        return query.FirstOrDefaultAsync();
    }

    public async Task<AuthResponse?> GetAuthenticateAsync(string username, string password)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .FirstOrDefaultAsync(a =>
                a.Username == username
                && !PasswordHasherHelper.VerifyPassword(password, a.HashedPassword)
                && a.DeletedAt == null
            );

        if (account == null)
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

    private Task<Account?> GetAccountByRefreshTokenAsync(string token)
    {
        var query = _unitOfWork.Accounts.GetQuery();

        return query.FirstOrDefaultAsync(a => a.RefreshToken == token);
    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(string refreshToken)
    {
        var account = await GetAccountByRefreshTokenAsync(refreshToken);

        if (account != null)
        {
            if (
                account.RefreshToken != null
                && account.RefreshTokenExpiryTime != null
                && account.RefreshTokenExpiryTime >= DateTime.UtcNow
            )
            {
                account.AccessToken = _jwtHelper.GenerateAccessToken(account);

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
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .FirstOrDefaultAsync(a => a.Id == accountId);

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

    private Task<Account?> GetAccountByEmailVerifiedCodeAsync(string code, VerificationType type)
    {
        var query = _unitOfWork.Accounts.GetQuery();

        return query.FirstOrDefaultAsync(a =>
            a.EmailVerifiedCode == code && a.VerificationType == type
        );
    }

    public async Task<bool> ConfirmRegisterAccountAsync(string confirmCode)
    {
        var account = await GetAccountByEmailVerifiedCodeAsync(
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
        var account = await GetAccountByEmailVerifiedCodeAsync(
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
        var account = await GetAccountByNameOrEmailAsync(identifier);

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
        var account = await GetAccountByEmailVerifiedCodeAsync(
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
