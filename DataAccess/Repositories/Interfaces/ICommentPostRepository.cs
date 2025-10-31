using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ICommentPictureRepository : IRepository<CommentPicture>
{
    Task<List<CommentPicture>> GetByCommentIdAsync(Guid commentId);
}
