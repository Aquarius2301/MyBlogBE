using System;
using BusinessObject.Enums;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using WebApi.Helpers;
using WebAPI.Dtos;
using WebAPI.Helpers;


namespace WebAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private JwtHelper _jwtHelper;
    private IUnitOfWork _unitOfWork;
    private readonly EmailHelper _emailHelper;
    public AuthService(JwtHelper jwtHelper, IUnitOfWork unitOfWork, EmailHelper emailHelper)
    {
        _jwtHelper = jwtHelper;
        _unitOfWork = unitOfWork;
        _emailHelper = emailHelper;
    }

    public async Task<AuthResponse?> GetAuthenticateAsync(string username, string password)
    {
        var account = await _unitOfWork.Accounts.GetByUsernameAsync(username);

        if (account == null || account.Status == StatusType.InActive
            || !PasswordHasher.VerifyPassword(password, account.HashedPassword))
        {
            return null;
        }

        account.AccessToken = _jwtHelper.GenerateAccessToken(account);
        account.RefreshToken = _jwtHelper.GenerateRefreshToken();
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtHelper.RefreshTokenDurationDays);
        await _unitOfWork.Accounts.UpdateAsync(account);

        return new AuthResponse
        {
            AccessToken = account.AccessToken,
            RefreshToken = account.RefreshToken
        };
    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(string refreshToken)
    {
        var account = await _unitOfWork.Accounts.GetByRefreshTokenAsync(refreshToken);

        if (account != null && account.Status != StatusType.InActive
             && account.RefreshToken == refreshToken)
        {
            if (account.RefreshTokenExpiryTime >= DateTime.UtcNow)
            {
                account.AccessToken = _jwtHelper.GenerateAccessToken(account);
                account.RefreshToken = _jwtHelper.GenerateRefreshToken();

                await _unitOfWork.Accounts.UpdateAsync(account);

                return new AuthResponse
                {
                    AccessToken = account.AccessToken,
                    RefreshToken = account.RefreshToken
                };
            }
            else
            {
                account.AccessToken = null;
                account.RefreshToken = null;
                account.RefreshTokenExpiryTime = null;

                await _unitOfWork.Accounts.UpdateAsync(account);

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

            await _unitOfWork.Accounts.UpdateAsync(account);

            return true;
        }

        return false;
    }

    public async Task<(bool, string?)> RegisterAccountAsync(RegisterRequest request)
    {
        if (await _unitOfWork.Accounts.GetByUsernameAsync(request.Username) != null)
        {
            return (false, "user");
        }

        if (await _unitOfWork.Accounts.GetByEmailAsync(request.Email) != null)
        {
            return (false, "email");
        }

        var tokenLength = _emailHelper.TokenLength;
        var tokenTimeout = _emailHelper.TokenTimeOut;
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
            HashedPassword = PasswordHasher.HashPassword(request.Password),
            Status = StatusType.InActive,
            CreatedAt = currentTime,
        };

        await _unitOfWork.Accounts.AddAsync(newAccount);

        return (true, confirmCode);
    }

    public async Task<bool> ConfirmRegisterAccountAsync(string confirmCode)
    {
        var account = await _unitOfWork.Accounts.GetByConfirmCodeAsync(confirmCode);

        if (account != null && account.VerificationType == VerificationType.Register
            && DateTime.UtcNow <= account.EmailVerifiedCodeExpiry)
        {
            account.EmailVerifiedCode = null;
            account.VerificationType = null;
            account.EmailVerifiedCodeExpiry = null;
            account.Status = StatusType.Active;

            await _unitOfWork.Accounts.UpdateAsync(account);
            return true;
        }

        return false;
    }

    public async Task<string?> ConfirmForgotPasswordAccountAsync(string confirmCode)
    {
        var account = await _unitOfWork.Accounts.GetByConfirmCodeAsync(confirmCode);

        if (account != null && account.VerificationType == VerificationType.ForgotPassword
            && DateTime.UtcNow <= account.EmailVerifiedCodeExpiry)
        {
            account.VerificationType = VerificationType.ChangePassword;
            account.EmailVerifiedCodeExpiry = null;

            await _unitOfWork.Accounts.UpdateAsync(account);
            return confirmCode;
        }

        return null;
    }

    public async Task<bool> ForgotPasswordAsync(string identifier)
    {
        var tokenTimeout = _emailHelper.TokenTimeOut;
        var account = await _unitOfWork.Accounts.GetQuery()
                        .FirstOrDefaultAsync(
                            x => (x.Email == identifier || x.Username == identifier)
                                && (x.Status != StatusType.InActive)
                                && x.DeletedAt == null
                        );

        if (account != null)
        {
            var confirmCode = StringHelper.GenerateRandomString(_emailHelper.TokenLength);
            await _emailHelper.SendForgotPasswordEmailAsync(account.Email, account.Username, confirmCode);

            account.EmailVerifiedCode = confirmCode;
            account.VerificationType = VerificationType.ForgotPassword;
            account.EmailVerifiedCodeExpiry = DateTime.UtcNow.AddMinutes(tokenTimeout);

            await _unitOfWork.Accounts.UpdateAsync(account);
            return true;
        }

        return false;
    }

    public async Task<bool> ResetPasswordAsync(string confirmCode, string newPassowrd)
    {
        var account = await _unitOfWork.Accounts.GetByConfirmCodeAsync(confirmCode);

        if (account != null && account.VerificationType == VerificationType.ChangePassword)
        {
            account.VerificationType = null;
            account.EmailVerifiedCodeExpiry = null;
            account.EmailVerifiedCode = null;
            account.HashedPassword = PasswordHasher.HashPassword(newPassowrd);

            await _unitOfWork.Accounts.UpdateAsync(account);

            return true;
        }

        return false;
    }
}
