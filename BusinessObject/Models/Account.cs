using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(Username), nameof(Email), IsUnique = true)]
public class Account
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(20)]
    public string Username { get; set; } = null!;

    [Required, MaxLength(50)]
    public string DisplayName { get; set; } = null!;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    public string Avatar { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = null!;
    public string? EmailVerifiedCode { get; set; } = null;
    public VerificationType? VerificationType { get; set; } = null;
    public DateTime? EmailVerifiedCodeExpiry { get; set; } = null;

    [Required, JsonIgnore]
    public string HashedPassword { get; set; } = null!;

    public string? RefreshToken { get; set; } = null;

    public string? AccessToken { get; set; } = null;

    public DateTime? RefreshTokenExpiryTime { get; set; } = null;
    public DateTime? SelfRemoveTime { get; set; } = null;

    [Required]
    public bool IsActive { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; } = null;

    public DateTime? DeletedAt { get; set; } = null;

    // Navigation
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
}
