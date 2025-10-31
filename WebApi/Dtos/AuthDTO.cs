using System;

namespace WebAPI.Dtos;

public class AuthRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public string? AccessToken { get; set; } = null!;
    public string? RefreshToken { get; set; } = null!;
}

public class UserInfoResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
}

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}