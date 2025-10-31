using System;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
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

        if (account == null || !PasswordHasher.VerifyPassword(password, account.HashedPassword))
        {
            return null;
        }

        account.AccessToken = _jwtHelper.GenerateAccessToken(account);
        account.RefreshToken = _jwtHelper.GenerateRefreshToken();
        account.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtHelper.RefreshTokenDurationDays);
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

        if (account != null && account.RefreshToken == refreshToken)
        {
            if (account.RefreshTokenExpiryTime >= DateTime.Now)
            {
                account.AccessToken = _jwtHelper.GenerateAccessToken(account);
                account.RefreshToken = _jwtHelper.GenerateRefreshToken();

                await _unitOfWork.Accounts.UpdateAsync(account);
            }
            else
            {
                account.AccessToken = null;
                account.RefreshToken = null;
                account.RefreshTokenExpiryTime = null;

                await _unitOfWork.Accounts.UpdateAsync(account);
            }

            return new AuthResponse
            {
                AccessToken = account.AccessToken,
                RefreshToken = account.RefreshToken
            };
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

    public async Task<string?> RegisterAccountAsync(RegisterRequest request)
    {

        var existingAccount = await _unitOfWork.Accounts.GetByUsernameAsync(request.Username);
        if (existingAccount != null)
        {
            return null;
        }
        var tokenLength = _emailHelper.TokenLength;
        var confirmCode = StringHelper.GenerateRandomString(tokenLength, true);

        await _emailHelper.SendEmailAsync(request.Email, request.Username, confirmCode);

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            DisplayName = request.DisplayName,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            EmailVerifiedCode = confirmCode,
            HashedPassword = PasswordHasher.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.Now,
        };

        await _unitOfWork.Accounts.AddAsync(newAccount);

        return confirmCode;
    }

    public async Task<bool> ConfirmAccountAsync(string confirmCode)
    {
        var tokenTimeout = _emailHelper.TokenTimeOut;
        var account = await _unitOfWork.Accounts.GetByConfirmCodeAsync(confirmCode);

        if (account != null && DateTime.Now <= account.CreatedAt.AddMinutes(tokenTimeout))
        {
            account.EmailVerifiedCode = null;

            await _unitOfWork.Accounts.UpdateAsync(account);
            return true;
        }

        return false;
    }
}
