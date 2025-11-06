using System;

namespace WebApi.Dtos;

public class GetPostsResponse
{
    public Guid Id { get; set; }
    public string Link { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> PostPicture { get; set; } = null!;
    public PostLatestComment? LatestComment { get; set; } = null;
    public bool IsFollowing { get; set; }
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int Score => (IsFollowing ? 30 : 0) + LikeCount * 2 + CommentCount * 3 + PostPicture.Count * 1;
}

public class PostLatestComment
{
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}


public class ABCRequest
{
    public string Title { get; set; }
    public IFormFile File { get; set; }
}
