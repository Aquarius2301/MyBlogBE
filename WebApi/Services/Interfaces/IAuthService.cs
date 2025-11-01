
using WebAPI.Dtos;

public interface IAuthService
{
    Task<AuthResponse?> GetAuthenticateAsync(string username, string password);
    Task<AuthResponse?> GetRefreshTokenAsync(string refreshToken);
    Task<bool> RemoveRefresh(Guid accountId);
    Task<(bool, string?)> RegisterAccountAsync(RegisterRequest request);
    Task<bool> ConfirmRegisterAccountAsync(string confirmCode);
    Task<string?> ConfirmForgetPasswordAccountAsync(string confirmCode);
    Task<bool> ForgotPasswordAsync(string identifier);
    Task<bool> ResetPasswordAsync(string confirmCode, string newPassowrd);
}