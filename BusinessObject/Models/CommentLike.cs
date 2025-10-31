using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(CommentId), nameof(AccountId), IsUnique = true)]
public class CommentLike
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CommentId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Comment Comment { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
