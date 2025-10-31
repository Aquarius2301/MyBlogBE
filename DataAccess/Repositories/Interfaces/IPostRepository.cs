using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<List<Post>> GetPostsByNameAsync(string name);
}
