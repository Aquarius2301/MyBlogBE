using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetChildCommentsAsync(Guid commentId);
}
