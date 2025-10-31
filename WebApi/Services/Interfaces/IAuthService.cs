
using WebAPI.Dtos;

public interface IAuthService
{
    Task<AuthResponse?> GetAuthenticateAsync(string username, string password);
    Task<AuthResponse?> GetRefreshTokenAsync(string refreshToken);
    Task<bool> RemoveRefresh(Guid accountId);
    Task<string?> RegisterAccountAsync(RegisterRequest request);
    Task<bool> ConfirmAccountAsync(string confirmCode);
}