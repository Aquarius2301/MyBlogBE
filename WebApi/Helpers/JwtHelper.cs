using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Dtos;
using WebApi.Settings;

namespace WebApi.Helpers;

/// <summary>
/// Helper class for generating JWT access tokens, refresh tokens,
/// and retrieving the current authenticated user's information.
/// </summary>
public class JwtHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly JwtSettings _settings;

    /// <summary>
    /// Gets the configured access token duration in minutes.
    /// </summary>
    public int AccessTokenDurationMinutes => _settings.AccessTokenDurationMinutes;

    /// <summary>
    /// Gets the configured refresh token duration in days.
    /// </summary>
    public int RefreshTokenDurationDays => _settings.RefreshTokenDurationDays;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtHelper"/> class.
    /// </summary>
    /// <param name="options">Options containing JWT settings.</param>
    /// <param name="httpContextAccessor">Accessor to retrieve the current HTTP context.</param>
    public JwtHelper(IOptions<JwtSettings> options, IHttpContextAccessor httpContextAccessor)
    {
        _settings = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Generates a signed JWT access token for the given account.
    /// </summary>
    /// <param name="account">The account for which the access token will be generated.</param>
    /// <returns>A JWT access token as a string.</returns>
    /// <remarks>
    /// The token includes claims for the account's Id and Username.
    /// Token expiration is based on the configured <see cref="AccessTokenDurationMinutes"/>.
    /// </remarks>
    public string GenerateAccessToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username),
            new Claim("Status", account.Status.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenDurationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a random string to be used as a refresh token.
    /// </summary>
    /// <returns>A secure random string representing the refresh token.</returns>
    public string GenerateRefreshToken()
    {
        return StringHelper.GenerateRandomString(64);
    }

    /// <summary>
    /// Retrieves information about the currently authenticated user from the HTTP context.
    /// </summary>
    /// <returns>
    /// A <see cref="UserInfoResponse"/> containing the user's Id and Username.
    /// </returns>
    /// <remarks>
    /// Returns default values if the user is not authenticated or claims are missing.
    /// </remarks>
    public UserInfoResponse GetAccountInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var accountId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var accountUsername = user?.FindFirst(ClaimTypes.Name)?.Value;
        var status = user?.FindFirst("Status")?.Value;

        return new UserInfoResponse
        {
            Id = Guid.Parse(accountId ?? ""),
            Username = accountUsername ?? "",
            StatusType = status != null ? Enum.Parse<StatusType>(status) : StatusType.InActive,
        };
    }
}
