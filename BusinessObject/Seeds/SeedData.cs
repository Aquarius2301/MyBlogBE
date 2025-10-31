using System;
using BusinessObject.Models;

namespace BusinessObject.Seeds;

public static class SeedData
{
    public static List<Account> CreateAccount()
    {
        return new List<Account>
        {
            new Account
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "admin",
            DisplayName = "Administrator",
            DateOfBirth = new DateOnly(2000, 1, 1),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "abc",
            CreatedAt = new DateTime(2025, 1, 15),
        },
        new Account
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Username = "khang",
            DisplayName = "Khang Ta",
            DateOfBirth = new DateOnly(2003, 8, 20),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "abcd",
            CreatedAt = new DateTime(2025, 3, 12),
        },
        new Account
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Username = "linh",
            DisplayName = "Nguyen Linh",
            DateOfBirth = new DateOnly(2001, 12, 5),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "abcas",
            CreatedAt = new DateTime(2024, 11, 10),
        },
        new Account
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Username = "nam",
            DisplayName = "Tran Nam",
            DateOfBirth = new DateOnly(1999, 4, 2),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "qwe",
            CreatedAt = new DateTime(2024, 12, 25),
        },
        new Account
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Username = "huong",
            DisplayName = "Le Huong",
            DateOfBirth = new DateOnly(2002, 7, 10),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "few",
            CreatedAt = new DateTime(2025, 5, 9),
        },
        new Account
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Username = "minh",
            DisplayName = "Pham Minh",
            DateOfBirth = new DateOnly(2000, 6, 15),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "qwe",
            CreatedAt = new DateTime(2025, 2, 20),
        },
        new Account
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Username = "thu",
            DisplayName = "Dang Thu",
            DateOfBirth = new DateOnly(2004, 3, 9),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "ergg",
            CreatedAt = new DateTime(2025, 6, 11),
        },
        new Account
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Username = "phuong",
            DisplayName = "Do Phuong",
            DateOfBirth = new DateOnly(1998, 5, 30),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "asdasd",
            CreatedAt = new DateTime(2024, 10, 2),
        },
        new Account
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Username = "an",
            DisplayName = "Bui An",
            DateOfBirth = new DateOnly(2003, 9, 1),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "cxz",
            CreatedAt = new DateTime(2025, 4, 3),
        },
        new Account
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Username = "dat",
            DisplayName = "Nguyen Dat",
            DateOfBirth = new DateOnly(2001, 11, 25),
            HashedPassword = "100000.2WBYMJzOMwL6A4WFMqocgA==.mOhKh2DlCdEv5kF51VSfWo9ddeeeayxz9kH7lwI4EAI=",
            IsActive = true,
            Email = "asdw",
            CreatedAt = new DateTime(2025, 7, 19),
        }
        };
    }

    public static List<Post> CreatePost()
    {
        return new List<Post>
        {
    new Post
    {
        Id = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // admin
        Link = "post-welcome",
        Content = "Chào mừng bạn đến với blog của chúng tôi!",
        CreatedAt = new DateTime(2025, 1, 20),
        UpdatedAt = new DateTime(2025, 1, 20),
    },
    new Post
    {
        Id = Guid.Parse("22222222-aaaa-aaaa-aaaa-222222222222"),
        AccountId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // khang
        Link = "post-csharp-tips",
        Content = "5 mẹo nhỏ giúp bạn viết C# sạch và hiệu quả hơn.",
        CreatedAt = new DateTime(2025, 3, 1),
        UpdatedAt = new DateTime(2025, 3, 1)
    },
    new Post
    {
        Id = Guid.Parse("33333333-aaaa-aaaa-aaaa-333333333333"),
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // linh
        Link = "post-efcore-seeding",
        Content = "Cách seed dữ liệu nhanh chóng trong EF Core.",
        CreatedAt = new DateTime(2025, 2, 12),
        UpdatedAt = new DateTime(2025, 2, 12)
    },
    new Post
    {
        Id = Guid.Parse("44444444-aaaa-aaaa-aaaa-444444444444"),
        AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // huong
        Link = "post-unity-start",
        Content = "Bắt đầu với Unity: Hướng dẫn tạo project đầu tiên.",
        CreatedAt = new DateTime(2025, 4, 5),
        UpdatedAt = new DateTime(2025, 4, 5)
    },
    new Post
    {
        Id = Guid.Parse("55555555-aaaa-aaaa-aaaa-555555555555"),
        AccountId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // minh
        Link = "post-aspnet-api",
        Content = "Giới thiệu cách xây dựng Web API với ASP.NET Core.",
        CreatedAt = new DateTime(2025, 4, 21),
        UpdatedAt = new DateTime(2025, 4, 21)
    },
    new Post
    {
        Id = Guid.Parse("66666666-aaaa-aaaa-aaaa-666666666666"),
        AccountId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // thu
        Link = "post-database-design",
        Content = "Các nguyên tắc cơ bản khi thiết kế cơ sở dữ liệu.",
        CreatedAt = new DateTime(2024, 12, 15),
        UpdatedAt = new DateTime(2024, 12, 15)
    },
    new Post
    {
        Id = Guid.Parse("77777777-aaaa-aaaa-aaaa-777777777777"),
        AccountId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // phuong
        Link = "post-jwt-auth",
        Content = "Giải thích cơ chế hoạt động của AccessToken và RefreshToken.",
        CreatedAt = new DateTime(2025, 5, 10),
        UpdatedAt = new DateTime(2025, 5, 10)
    },
    new Post
    {
        Id = Guid.Parse("88888888-aaaa-aaaa-aaaa-888888888888"),
        AccountId = Guid.Parse("99999999-9999-9999-9999-999999999999"), // an
        Link = "post-mobile-programming",
        Content = "Lập trình di động: Bắt đầu với Flutter và React Native.",
        CreatedAt = new DateTime(2025, 6, 18),
        UpdatedAt = new DateTime(2025, 6, 18)
    },
    new Post
    {
        Id = Guid.Parse("99999999-aaaa-aaaa-aaaa-999999999999"),
        AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // dat
        Link = "post-clean-architecture",
        Content = "Clean Architecture là gì và tại sao nên áp dụng?",
        CreatedAt = new DateTime(2024, 11, 30),
        UpdatedAt = new DateTime(2024, 11, 30)
    },
    new Post
    {
        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0000"),
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // linh có 2 post
        Link = "post-sql-cascade",
        Content = "Tìm hiểu về cascade delete trong SQL và EF Core.",
        CreatedAt = new DateTime(2025, 8, 2),
        UpdatedAt = new DateTime(2025, 8, 2)
    }
        };
    }

    public static List<Comment> CreateComment()
    {
        return new List<Comment>
        {
            new Comment
            {
                Id = Guid.Parse("11111111-bbbb-bbbb-bbbb-111111111111"),
                AccountId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // khang
                ParentCommentId = null,
                ReplyAccountId = null,
                PostId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"), // post-welcome
                Content = "Bài viết hay quá!",
                CreatedAt = new DateTime(2025, 1, 21),
                UpdatedAt = new DateTime(2025, 1, 21)
            },
    // --- Comment 2 ---
    new Comment
    {
        Id = Guid.Parse("22222222-bbbb-bbbb-bbbb-222222222222"),
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // linh
        ParentCommentId = Guid.Parse("11111111-bbbb-bbbb-bbbb-111111111111"),
        ReplyAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        PostId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"),
        Content = "Đồng ý luôn!",
        CreatedAt = new DateTime(2025, 1, 21),
        UpdatedAt = new DateTime(2025, 1, 21)
    },
    // --- Comment 3 ---
    new Comment
    {
        Id = Guid.Parse("33333333-bbbb-bbbb-bbbb-333333333333"),
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // nam
        ParentCommentId = Guid.Parse("11111111-bbbb-bbbb-bbbb-111111111111"),
        ReplyAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        PostId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"),
        Content = "Cảm ơn vì chia sẻ!",
        CreatedAt = new DateTime(2025, 1, 22),
        UpdatedAt = new DateTime(2025, 1, 22)
    },
    // --- Comment 4 ---
    new Comment
    {
        Id = Guid.Parse("44444444-bbbb-bbbb-bbbb-444444444444"),
        AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // huong
        ParentCommentId = Guid.Parse("11111111-bbbb-bbbb-bbbb-111111111111"),
        ReplyAccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        PostId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"),
        Content = "Chuẩn, bài chi tiết lắm!",
        CreatedAt = new DateTime(2025, 1, 23),
        UpdatedAt = new DateTime(2025, 1, 23)
    },
    // --- Comment 5 ---
    new Comment
    {
        Id = Guid.Parse("55555555-bbbb-bbbb-bbbb-555555555555"),
        AccountId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // minh
        ParentCommentId = null,
        ReplyAccountId = null,
        PostId = Guid.Parse("22222222-aaaa-aaaa-aaaa-222222222222"), // post-csharp-tips
        Content = "Hướng dẫn dễ hiểu quá!",
        CreatedAt = new DateTime(2025, 3, 3),
        UpdatedAt = new DateTime(2025, 3, 3)
    },
    // --- Comment 6 ---
    new Comment
    {
        Id = Guid.Parse("66666666-bbbb-bbbb-bbbb-666666666666"),
        AccountId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // thu
        ParentCommentId = null,
        ReplyAccountId = null,
        PostId = Guid.Parse("33333333-aaaa-aaaa-aaaa-333333333333"), // post-efcore-seeding
        Content = "EF Core seeding hay đó!",
        CreatedAt = new DateTime(2025, 3, 5),
        UpdatedAt = new DateTime(2025, 3, 5)
    },
    // --- Comment 7 ---
    new Comment
    {
        Id = Guid.Parse("77777777-bbbb-bbbb-bbbb-777777777777"),
        AccountId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // phuong
        ParentCommentId = Guid.Parse("66666666-bbbb-bbbb-bbbb-666666666666"),
        ReplyAccountId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
        PostId = Guid.Parse("33333333-aaaa-aaaa-aaaa-333333333333"),
        Content = "Mình làm thử thành công rồi!",
        CreatedAt = new DateTime(2025, 3, 6),
        UpdatedAt = new DateTime(2025, 3, 6)
    },
    // --- Comment 8 ---
    new Comment
    {
        Id = Guid.Parse("88888888-bbbb-bbbb-bbbb-888888888888"),
        AccountId = Guid.Parse("99999999-9999-9999-9999-999999999999"), // an
        ParentCommentId = null,
        ReplyAccountId = null,
        PostId = Guid.Parse("44444444-aaaa-aaaa-aaaa-444444444444"), // post-unity-start
        Content = "Unity cơ bản cực dễ hiểu luôn!",
        CreatedAt = new DateTime(2025, 4, 6),
        UpdatedAt = new DateTime(2025, 4, 6)
    },
    // --- Comment 9 ---
    new Comment
    {
        Id = Guid.Parse("99999999-bbbb-bbbb-bbbb-999999999999"),
        AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // dat
        ParentCommentId = null,
        ReplyAccountId = null,
        PostId = Guid.Parse("55555555-aaaa-aaaa-aaaa-555555555555"), // post-aspnet-api
        Content = "Cảm ơn bài hướng dẫn!",
        CreatedAt = new DateTime(2025, 5, 2),
        UpdatedAt = new DateTime(2025, 5, 2)
    },
    // --- Comment 10 ---
    new Comment
    {
        Id = Guid.Parse("aaaaaaaa-bbbb-bbbb-bbbb-aaaaaaaaaaaa"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // admin
        ParentCommentId = Guid.Parse("99999999-bbbb-bbbb-bbbb-999999999999"),
        ReplyAccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        PostId = Guid.Parse("55555555-aaaa-aaaa-aaaa-555555555555"),
        Content = "Không có chi!",
        CreatedAt = new DateTime(2025, 5, 3),
        UpdatedAt = new DateTime(2025, 5, 3)
    }

        };

    }

    public static List<PostLike> CreatePostLike()
    {
        return new List<PostLike>
        {
            new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333001"),
        PostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0000"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CreatedAt = new DateTime(2024, 10, 10)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333002"),
        PostId = Guid.Parse("99999999-AAAA-AAAA-AAAA-999999999999"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CreatedAt = new DateTime(2024, 10, 11)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333003"),
        PostId = Guid.Parse("33333333-AAAA-AAAA-AAAA-333333333333"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CreatedAt = new DateTime(2024, 10, 9)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333004"),
        PostId = Guid.Parse("22222222-AAAA-AAAA-AAAA-222222222222"),
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        CreatedAt = new DateTime(2024, 10, 8)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333005"),
        PostId = Guid.Parse("88888888-AAAA-AAAA-AAAA-888888888888"),
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        CreatedAt = new DateTime(2024, 10, 12)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333006"),
        PostId = Guid.Parse("88888888-AAAA-AAAA-AAAA-888888888888"),
        AccountId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
        CreatedAt = new DateTime(2024, 10, 13)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333007"),
        PostId = Guid.Parse("11111111-AAAA-AAAA-AAAA-111111111111"),
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        CreatedAt = new DateTime(2024, 10, 14)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333008"),
        PostId = Guid.Parse("22222222-AAAA-AAAA-AAAA-222222222222"),
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        CreatedAt = new DateTime(2024, 10, 14)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333009"),
        PostId = Guid.Parse("88888888-AAAA-AAAA-AAAA-888888888888"),
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        CreatedAt = new DateTime(2024, 10, 15)
    },
    new PostLike
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333010"),
        PostId = Guid.Parse("55555555-AAAA-AAAA-AAAA-555555555555"),
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        CreatedAt = new DateTime(2024, 10, 16)
    }
        };
    }

    public static List<CommentLike> CreateCommentLike()
    {
        return new List<CommentLike>{
            new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555001"),
        CommentId = Guid.Parse("11111111-BBBB-BBBB-BBBB-111111111111"),
        AccountId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
        CreatedAt = new DateTime(2024, 10, 10)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555002"),
        CommentId = Guid.Parse("66666666-BBBB-BBBB-BBBB-666666666666"),
        AccountId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
        CreatedAt = new DateTime(2024, 10, 11)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555003"),
        CommentId = Guid.Parse("88888888-BBBB-BBBB-BBBB-888888888888"),
        AccountId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
        CreatedAt = new DateTime(2024, 10, 9)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555004"),
        CommentId = Guid.Parse("22222222-BBBB-BBBB-BBBB-222222222222"),
        AccountId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
        CreatedAt = new DateTime(2024, 10, 8)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555005"),
        CommentId = Guid.Parse("33333333-BBBB-BBBB-BBBB-333333333333"),
        AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        CreatedAt = new DateTime(2024, 10, 12)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555006"),
        CommentId = Guid.Parse("11111111-BBBB-BBBB-BBBB-111111111111"),
        AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        CreatedAt = new DateTime(2024, 10, 13)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555007"),
        CommentId = Guid.Parse("44444444-BBBB-BBBB-BBBB-444444444444"),
        AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        CreatedAt = new DateTime(2024, 10, 14)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555008"),
        CommentId = Guid.Parse("88888888-BBBB-BBBB-BBBB-888888888888"),
        AccountId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
        CreatedAt = new DateTime(2024, 10, 15)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555009"),
        CommentId = Guid.Parse("44444444-BBBB-BBBB-BBBB-444444444444"),
        AccountId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
        CreatedAt = new DateTime(2024, 10, 16)
    },
    new CommentLike
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555010"),
        CommentId = Guid.Parse("11111111-BBBB-BBBB-BBBB-111111111111"),
        AccountId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
        CreatedAt = new DateTime(2024, 10, 17)
    }
        }
        ;
    }

    public static List<Follow> CreateFollow()
    {
        return new List<Follow>{
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0001"),
                AccountId =  Guid.Parse("22222222-2222-2222-2222-222222222222"), // khang
                FollowingId =  Guid.Parse("11111111-1111-1111-1111-111111111111") // admin
            },
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0002"),
                AccountId =  Guid.Parse("33333333-3333-3333-3333-333333333333"), // linh
                FollowingId =  Guid.Parse("22222222-2222-2222-2222-222222222222") // khang
            },
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0003"),
                AccountId =  Guid.Parse("44444444-4444-4444-4444-444444444444"), // nam
                FollowingId =  Guid.Parse("33333333-3333-3333-3333-333333333333") // linh
            },
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0004"),
                AccountId =  Guid.Parse("55555555-5555-5555-5555-555555555555"), // huong
                FollowingId =  Guid.Parse("44444444-4444-4444-4444-444444444444") // nam
            },
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0005"),
                AccountId =  Guid.Parse("66666666-6666-6666-6666-666666666666"), // minh
                FollowingId =  Guid.Parse("55555555-5555-5555-5555-555555555555") // huong
            },
            new Follow
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0006"),
                AccountId =  Guid.Parse("77777777-7777-7777-7777-777777777777"), // thu
                FollowingId =  Guid.Parse("66666666-6666-6666-6666-666666666666") // minh
            },
        };
    }
}