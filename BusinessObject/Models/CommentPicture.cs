using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(Link), IsUnique = true)]
/// <summary>
/// Represents a picture attached to a comment.
/// </summary>
public class CommentPicture
{
    /// <summary>
    /// Unique identifier for the comment picture.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the comment this picture belongs to.
    /// </summary>
    [Required]
    public Guid CommentId { get; set; }

    /// <summary>
    /// URL or path to the picture.
    /// </summary>
    [Required]
    public string Link { get; set; } = null!;

    /// <summary>
    /// Timestamp when the picture was added.
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
    /// The comment associated with this picture.
    /// </summary>
    public Comment Comment { get; set; } = null!;

    #endregion
}