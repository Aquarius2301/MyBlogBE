using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Dtos;

namespace WebAPI.Helpers;

/// <summary>
/// Configuration settings for JWT token generation and validation.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The secret key used to sign JWT tokens.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// The issuer of the JWT token.
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// The audience of the JWT token.
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Duration of the access token in minutes.
    /// </summary>
    public int AccessTokenDurationMinutes { get; set; }

    /// <summary>
    /// Duration of the refresh token in days.
    /// </summary>
    public int RefreshTokenDurationDays { get; set; }
}

/// <summary>
/// Helper class for generating JWT access tokens, refresh tokens, 
/// and retrieving the current authenticated user's information.
/// </summary>
public class JwtHelper
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Gets the configured access token duration in minutes.
    /// </summary>
    public int AccessTokenDurationMinutes => _config.GetValue<int>("JwtSettings:AccessTokenDurationMinutes");

    /// <summary>
    /// Gets the configured refresh token duration in days.
    /// </summary>
    public int RefreshTokenDurationDays => _config.GetValue<int>("JwtSettings:RefreshTokenDurationDays");

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtHelper"/> class.
    /// </summary>
    /// <param name="config">Application configuration containing JWT settings.</param>
    /// <param name="unitOfWork">Unit of work for database access (optional, if needed for future methods).</param>
    /// <param name="httpContextAccessor">Accessor to retrieve the current HTTP context.</param>
    public JwtHelper(IConfiguration config, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
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
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenDurationMinutes),
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

        return new UserInfoResponse
        {
            Id = Guid.Parse(accountId ?? ""),
            Username = accountUsername ?? ""
        };
    }
}

