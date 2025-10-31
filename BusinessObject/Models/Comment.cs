
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Models;

public class Comment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ParentCommentId { get; set; } = null;

    public Guid? ReplyAccountId { get; set; } = null;

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; } = null;

    // Navigation
    public Account Account { get; set; } = null!;
    public Account? ReplyAccount { get; set; } = null;
    public Comment? ParentComment { get; set; } = null;
    public Post Post { get; set; } = null!;
    public ICollection<CommentPicture> CommentPictures { get; set; } = new List<CommentPicture>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

}
