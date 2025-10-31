using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPostPictureRepository : IRepository<PostPicture>
{
    Task<List<PostPicture>> GetByPostIdAsync(Guid postId);
}
