using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Dtos;

namespace WebAPI.Helpers;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenDurationMinutes { get; set; }
    public int RefreshTokenDurationDays { get; set; }
}


public class JwtHelper
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public int AccessTokenDurationMinutes => _config.GetValue<int>("JwtSettings:AccessTokenDurationMinutes");
    public int RefreshTokenDurationDays => _config.GetValue<int>("JwtSettings:RefreshTokenDurationDays");

    public JwtHelper(IConfiguration config, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

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

    public string GenerateRefreshToken()
    {
        return StringHelper.GenerateRandomString(64);
    }

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
