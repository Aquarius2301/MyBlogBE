using System;
using WebApi.Dtos;

namespace WebApi.Services.Interfaces;

public interface IPostService
{
    Task<List<GetPostsResponse>> GetPostsListAsync(DateTime? cursor, Guid userId, int pageSize);
    Task<List<GetMyPostsResponse>> GetMyPostsListAsync(DateTime? cursor, Guid userId, int pageSize);
    Task<GetPostDetailResponse> GetPostByLinkAsync(string link, Guid userId);
    Task<bool> ToggleLikePostAsync(Guid postId, Guid userId);
}
