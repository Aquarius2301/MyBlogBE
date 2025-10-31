using System;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    ICommentRepository Comments { get; }
    ICommentLikeRepository CommentLikes { get; }
    ICommentPictureRepository CommentPictures { get; }
    IPostRepository Posts { get; }
    IPostLikeRepository PostLikes { get; }
    IPostPictureRepository PostPictures { get; }

    Task BeginTransactionAsync();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
}
