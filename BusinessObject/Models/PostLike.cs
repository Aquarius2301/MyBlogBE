using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(PostId), nameof(AccountId), IsUnique = true)]
public class PostLike
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Post Post { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
