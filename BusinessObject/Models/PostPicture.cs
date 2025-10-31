using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(Link), IsUnique = true)]
public class PostPicture
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public string Link { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; } = null;

    // Navigation
    public Post Post { get; set; } = null!;

}
