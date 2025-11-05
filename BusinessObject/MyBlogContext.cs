using BusinessObject.Models;
using BusinessObject.Seeds;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject;

public class MyBlogContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<CommentPicture> CommentPictures { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<PostPicture> PostPictures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Posts)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Comments)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Account)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ReplyAccount)
            .WithMany()
            .HasForeignKey(c => c.ReplyAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(c => c.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CommentLike>()
            .HasOne(c => c.Comment)
            .WithMany(c => c.CommentLikes)
            .HasForeignKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CommentLike>()
            .HasOne(c => c.Account)
            .WithMany(c => c.CommentLikes)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CommentPicture>()
            .HasOne(c => c.Comment)
            .WithMany(c => c.CommentPictures)
            .HasForeignKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Follow>()
            .HasOne(c => c.Account)
            .WithMany(c => c.Following)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Follow>()
            .HasOne(c => c.Following)
            .WithMany(c => c.Followers)
            .HasForeignKey(c => c.FollowingId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Post>()
            .HasOne(c => c.Account)
            .WithMany(c => c.Posts)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Post>()
            .HasMany(c => c.PostPictures)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Post>()
            .HasMany(c => c.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PostLike>()
            .HasOne(c => c.Post)
            .WithMany(c => c.PostLikes)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PostLike>()
            .HasOne(c => c.Account)
            .WithMany(c => c.PostLikes)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PostPicture>()
            .HasOne(c => c.Post)
            .WithMany(c => c.PostPictures)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public MyBlogContext(DbContextOptions<MyBlogContext> options)
        : base(options)
    {

    }
}
