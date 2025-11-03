using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(Link), IsUnique = true)]
/// <summary>
/// Represents a picture attached to a post.
/// </summary>
public class PostPicture
{
    /// <summary>
    /// Unique identifier for the post picture.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the post this picture belongs to.
    /// </summary>
    [Required]
    public Guid PostId { get; set; }

    /// <summary>
    /// URL or path of the picture.
    /// </summary>
    [Required]
    public string Link { get; set; } = null!;

    /// <summary>
    /// Timestamp when the picture was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of the last update to the picture.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when the picture was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; } = null;

    #region Navigation Properties

    /// <summary>
    /// The post associated with this picture.
    /// </summary>
    public Post Post { get; set; } = null!;

    #endregion
}