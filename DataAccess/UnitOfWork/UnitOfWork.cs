using System;
using BusinessObject;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly MyBlogContext _context;
    private IDbContextTransaction _transaction = null!; // No injection for transaction
    public IAccountRepository Accounts { get; private set; }
    public ICommentRepository Comments { get; private set; }
    public ICommentLikeRepository CommentLikes { get; private set; }
    public ICommentPictureRepository CommentPictures { get; private set; }
    public IPostRepository Posts { get; private set; }
    public IPostLikeRepository PostLikes { get; private set; }
    public IPostPictureRepository PostPictures { get; private set; }

    public UnitOfWork(MyBlogContext context,
                        IAccountRepository accountRepository,
                        ICommentRepository commentRepository,
                        ICommentLikeRepository commentLikeRepository,
                        ICommentPictureRepository commentPictureRepository,
                        IPostRepository postRepository,
                        IPostLikeRepository postLikeRepository,
                        IPostPictureRepository postPictureRepository)
    {
        _context = context;
        Accounts = accountRepository;
        Comments = commentRepository;
        CommentLikes = commentLikeRepository;
        CommentPictures = commentPictureRepository;
        Posts = postRepository;
        PostLikes = postLikeRepository;
        PostPictures = postPictureRepository;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _transaction.CommitAsync();
    }
}
