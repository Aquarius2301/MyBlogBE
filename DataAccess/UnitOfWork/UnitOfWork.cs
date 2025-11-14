using BusinessObject;
using DataAccess.Repositories;

namespace DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IAccountRepository Accounts { get; }
    public IPictureRepository Pictures { get; }
    public IPostRepository Posts { get; }
    public IPostLikeRepository PostLikes { get; }
    public ICommentRepository Comments { get; }
    public ICommentLikeRepository CommentLikes { get; }
    private readonly MyBlogContext _context;

    public UnitOfWork(
        MyBlogContext context,
        IAccountRepository accountRepository,
        IPictureRepository pictureRepository,
        IPostRepository postRepository,
        IPostLikeRepository postLikeRepository,
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository
    )
    {
        _context = context;
        Accounts = accountRepository;
        Pictures = pictureRepository;
        Posts = postRepository;
        PostLikes = postLikeRepository;
        Comments = commentRepository;
        CommentLikes = commentLikeRepository;
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
