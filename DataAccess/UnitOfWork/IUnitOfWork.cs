using System;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    ICommentRepository Comments { get; }
    ICommentLikeRepository CommentLikes { get; }
    IPostRepository Posts { get; }
    IPostLikeRepository PostLikes { get; }

    Task BeginTransactionAsync();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
}
