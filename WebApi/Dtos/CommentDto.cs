namespace WebApi.Dtos;

public class GetCommentsResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string AccountAvatar { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> CommentPictures { get; set; } = null!;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    public int Score => CommentCount * 3 + LikeCount * 2 + CommentPictures.Count;
}

public class GetChildCommentsResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string AccountAvatar { get; set; } = null!;
    public string ReplyAccountName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> CommentPictures { get; set; } = null!;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    // public int Score => CommentCount * 3 + LikeCount * 2 + CommentPictures.Count;
}

public class CreateCommentRequest
{
    public string Content { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ReplyAccountId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}

public class CreateCommentResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Content { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ReplyAccountId { get; set; }
    public List<string> Pictures { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = null!;
    public List<IFormFile> Images { get; set; } = [];

    public bool ClearImages { get; set; } = false; // true to clear all images
}

public class UpdateCommentResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public List<string> Pictures { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
}
