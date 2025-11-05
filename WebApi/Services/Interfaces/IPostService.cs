using System;
using WebApi.Dtos;

namespace WebApi.Services.Interfaces;

public interface IPostService
{
    Task<List<GetPostsResponse>> GetPostsListAsync(DateTime? cursor, Guid userId, int pageSize);
}
