namespace WebApi.Dtos;

public class GetPostsResponse
{
    public Guid Id { get; set; }
    public string Link { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> PostPictures { get; set; } = null!;
    public PostLatestComment? LatestComment { get; set; } = null;
    public bool IsFollowing { get; set; }
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int Score =>
        (IsFollowing ? 30 : 0) + LikeCount * 2 + CommentCount * 3 + PostPictures.Count;
}

public class GetMyPostsResponse
{
    public Guid Id { get; set; }
    public string Link { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> PostPicture { get; set; } = null!;
    public PostLatestComment? LatestComment { get; set; } = null;
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class PostLatestComment
{
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class GetPostDetailResponse
{
    public Guid Id { get; set; }
    public string Link { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> PostPicture { get; set; } = null!;
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class CreatePostRequest
{
    public string Content { get; set; } = null!;
}

public class CreatePostResponse
{
    public Guid Id { get; set; }
    public string Link { get; set; } = null!;
    public string Content { get; set; } = null!;
}


// public class PostComment
// {
//     public Guid Id { get; set; }
//     public string Username { get; set; } = null!;
//     public string DisplayName { get; set; } = null!;
//     public string Content { get; set; } = null!;
//     public int LikeCount { get; set; }
//     public int CommentCount { get; set; }
//     public bool IsLiked { get; set; }
//     public DateTime CreatedAt { get; set; }
// }
