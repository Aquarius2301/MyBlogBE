using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(Link), IsUnique = true)]
public class Post
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public string Link { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; } = null;

    // Navigation
    public Account Account { get; set; } = null!;
    public ICollection<PostPicture> PostPictures { get; set; } = new List<PostPicture>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}
