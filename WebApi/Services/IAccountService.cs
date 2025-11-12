using System;
using BusinessObject.Models;
using WebApi.Dtos;

namespace WebApi.Services;

public interface IAccountService
{
    Task<AccountResponse?> GetProfileByIdAsync(Guid accountId);

    Task<List<AccountNameResponse>> GetAccountByNameAsync(
        string name,
        DateTime? cursor,
        int pageSize
    );

    Task<UpdateAccountResponse> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request);
    Task<bool> ChangePasswordAsync(Guid accountId, UpdatePasswordRequest request);
    Task<bool> IsPasswordCorrectAsync(Guid accountId, string password);
}
