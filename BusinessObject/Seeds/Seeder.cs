using BusinessObject.Enums;
using BusinessObject.Models;

namespace BusinessObject.Seeds;


public class Seeder
{
    public static void Seed(MyBlogContext context)
    {
        CreateAccounts(context);
        CreateFollows(context);
        CreatePosts(context);
        CreatePostPictures(context);
        CreatePostLikes(context);
        CreateComments(context);
        CreateCommentPictures(context);
    }


    private static void CreateAccounts(MyBlogContext context)
    {
        if (!context.Accounts.Any())
        {
            Console.WriteLine("Seeding accounts...");
            var accounts = new List<Account>();
            for (int i = 0; i < 20; i++)
            {
                accounts.Add(new Account
                {
                    Id = Guid.NewGuid(),
                    Username = $"user{i + 1}",
                    DisplayName = $"User {i + 1}",
                    DateOfBirth = new DateOnly(1990 + (i % 10), 1 + (i % 12), 1 + (i % 28)),
                    HashedPassword =
                        "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
                    Status = StatusType.Active,
                    Email = $"user{i + 1}@example.com",

                    CreatedAt = DateTime.UtcNow.AddDays(Random.Shared.Next(-100, 0)),
                });
            }

            context.Accounts.AddRange(accounts);
            context.SaveChanges();


        }
    }

    private static void CreatePosts(MyBlogContext context)
    {
        if (!context.Posts.Any())
        {
            Console.WriteLine("Seeding posts...");

            var posts = new List<Post>();
            var accounts = context.Accounts.ToList();

            foreach (var account in accounts)
            {
                var numOfPosts = Random.Shared.Next(1, 6);
                for (int i = 0; i < numOfPosts; i++)
                {
                    posts.Add(new Post
                    {
                        Id = Guid.NewGuid(),
                        Link = $"/post/{account.Username}/{i + 1}",
                        Content = $"This is post {i + 1} by {account.DisplayName}.",
                        AccountId = account.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(Random.Shared.Next(-100, 0)),
                    });
                }
            }

            context.Posts.AddRange(posts);
            context.SaveChanges();
        }
    }

    private static void CreatePostPictures(MyBlogContext context)
    {
        if (!context.PostPictures.Any())
        {
            Console.WriteLine("Seeding post pictures...");

            var postPictures = new List<PostPicture>();
            var posts = context.Posts.ToList();

            foreach (var post in posts)
            {
                var numOfPosts = Random.Shared.Next(1, 10);
                for (int i = 0; i < numOfPosts; i++)
                {
                    postPictures.Add(new PostPicture
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        Link = $"/{post.Id}/picture{i + 1}.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(Random.Shared.Next(-100, 0)),
                    });
                }
            }

            context.PostPictures.AddRange(postPictures);
            context.SaveChanges();
        }
    }

    private static void CreatePostLikes(MyBlogContext context)
    {
        if (!context.PostLikes.Any())
        {
            Console.WriteLine("Seeding post likes...");

            var postLikes = new List<PostLike>();
            var posts = context.Posts.ToList();
            var accounts = context.Accounts.ToList();
            var rand = new Random();

            foreach (var post in posts)
            {
                int likeCount = rand.Next(1, 4);

                var likedAccounts = accounts.OrderBy(a => rand.Next()).Take(likeCount).ToList();

                foreach (var acc in likedAccounts)
                {
                    postLikes.Add(new PostLike
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        AccountId = acc.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(Random.Shared.Next(-100, 0))
                    });
                }
            }

            context.PostLikes.AddRange(postLikes);
            context.SaveChanges();
        }
    }

    private static void CreateFollows(MyBlogContext context)
    {
        if (!context.Follows.Any())
        {
            Console.WriteLine("Seeding follows...");

            var existingAccounts = context.Accounts.ToList();
            var follows = new List<Follow>();
            var rand = new Random();

            foreach (var acc in existingAccounts)
            {
                int followCount = rand.Next(1, 4);

                var followAccounts = existingAccounts.OrderBy(a => rand.Next()).Take(followCount).ToList();

                foreach (var followAcc in followAccounts)
                {
                    follows.Add(new Follow
                    {
                        Id = Guid.NewGuid(),
                        FollowingId = followAcc.Id,
                        AccountId = acc.Id,
                    });
                }
            }

            context.Follows.AddRange(follows);
            context.SaveChanges();
        }
    }

    private static void CreateComments(MyBlogContext context)
    {
        if (!context.Comments.Any())
        {
            Console.WriteLine("Seeding comments...");

            var rand = new Random();
            var comments = new List<Comment>();
            var posts = context.Posts.ToList();
            var accounts = context.Accounts.ToList();

            foreach (var post in posts)
            {
                var commentCount = rand.Next(1, 5);
                for (int i = 0; i < commentCount; i++)
                {
                    var account = accounts[rand.Next(accounts.Count)];

                    var parentComment = new Comment
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        ParentCommentId = null,
                        ReplyAccountId = null,
                        PostId = post.Id,
                        Content = $"Parent comment from {account.Username}",
                        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(100))
                    };

                    comments.Add(parentComment);

                    int childCount = rand.Next(1, 10);
                    for (int j = 0; i < childCount; i++)
                    {
                        var childAccount = accounts[rand.Next(accounts.Count)];

                        comments.Add(new Comment
                        {
                            Id = Guid.NewGuid(),
                            AccountId = childAccount.Id,
                            ParentCommentId = parentComment.Id,
                            ReplyAccountId = parentComment.AccountId,
                            PostId = post.Id,
                            Content = $"Reply comment {j + 1} from {childAccount.Username}",
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(100)),
                            UpdatedAt = DateTime.UtcNow.AddDays(-rand.Next(100))
                        });
                    }
                }
            }

            context.Comments.AddRange(comments);
            context.SaveChanges();
        }
    }

    private static void CreateCommentPictures(MyBlogContext context)
    {
        if (!context.CommentPictures.Any())
        {
            Console.WriteLine("Seeding comment pictures...");

            var rand = new Random();
            var commentPics = new List<CommentPicture>();
            var comments = context.Comments.ToList();

            foreach (var comment in comments)
            {
                var commentPicsCount = rand.Next(1, 5);
                for (int i = 0; i < commentPicsCount; i++)
                {
                    commentPics.Add(new CommentPicture
                    {
                        Id = Guid.NewGuid(),
                        CommentId = comment.Id,
                        Link = $"/comments/{comment.Id}/picture{i + 1}.jpg",
                        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(100))
                    });


                }
            }
            context.CommentPictures.AddRange(commentPics);
            context.SaveChanges();
        }
    }


}