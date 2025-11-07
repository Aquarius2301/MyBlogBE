using System;

namespace WebApi.Dtos;

public class GetCommentsResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
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
    public string ReplyAccountName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> CommentPictures { get; set; } = null!;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    // public int Score => CommentCount * 3 + LikeCount * 2 + CommentPictures.Count;

}

