using System;
using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using WebApi.Dtos;
using WebApi.Helpers;

namespace WebApi.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IBaseRepository _repository;

    public AccountService(IBaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AccountNameResponse>> GetAccountByNameAsync(
        string name,
        DateTime? cursor,
        int pageSize
    )
    {
        var account = await _repository
            .Accounts.GetQuery()
            .Where(a =>
                (a.Username.Contains(name) || a.DisplayName.Contains(name))
                && (cursor == null || a.CreatedAt < cursor)
            )
            .Select(a => new AccountNameResponse
            {
                Id = a.Id,
                Username = a.Username,
                DisplayName = a.DisplayName,
                CreatedAt = a.CreatedAt,
            })
            .OrderByDescending(a => a.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        return account;
    }

    public async Task<AccountResponse?> GetProfileByIdAsync(Guid accountId)
    {
        var account = await _repository
            .Accounts.GetQuery()
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                DisplayName = a.DisplayName,
                DateOfBirth = a.DateOfBirth,
                AvatarUrl = a.Picture != null ? a.Picture.Link : "",
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync(x => x.Id == accountId);

        return account;
    }

    public async Task<UpdateAccountResponse> UpdateAccountAsync(
        Guid accountId,
        UpdateAccountRequest request
    )
    {
        var account = await _repository.Accounts.GetByIdAsync(accountId);

        if (account == null)
            throw new Exception("Account not found"); // User can be deleted while updating

        account.Username = request.Username ?? account.Username;
        account.DisplayName = request.DisplayName ?? account.DisplayName;
        account.DateOfBirth = request.DateOfBirth ?? account.DateOfBirth;

        await _repository.SaveChangesAsync();

        return new UpdateAccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            DisplayName = account.DisplayName,
            DateOfBirth = account.DateOfBirth,
        };
    }

    public Task<bool> ChangePasswordAsync(Guid accountId, UpdatePasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsPasswordCorrectAsync(Guid accountId, string password)
    {
        var account = await _repository.Accounts.GetByIdAsync(accountId);

        if (
            account != null
            && PasswordHasherHelper.VerifyPassword(password, account.HashedPassword)
        )
        {
            return true;
        }

        return false;
    }
}
