using DataAccess.Repositories;

namespace DataAccess.UnitOfWork;

/// <summary>
/// Defines the unit of work interface for managing repositories and database transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the account repository.
    /// </summary>
    IAccountRepository Accounts { get; }

    /// <summary>
    /// Gets the picture repository.
    /// </summary>
    IPictureRepository Pictures { get; }

    /// <summary>
    /// Gets the post repository.
    /// </summary>
    IPostRepository Posts { get; }

    /// <summary>
    /// Gets the post like repository.
    /// </summary>
    IPostLikeRepository PostLikes { get; }

    /// <summary>
    /// Gets the comment repository.
    /// </summary>
    ICommentRepository Comments { get; }

    /// <summary>
    /// Gets the comment like repository.
    /// </summary>
    ICommentLikeRepository CommentLikes { get; }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns></returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    /// <returns></returns>
    Task CommitTransactionAsync();

    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
