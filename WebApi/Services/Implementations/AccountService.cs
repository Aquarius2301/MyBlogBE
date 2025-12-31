using BusinessObject.Models;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Settings;

namespace WebApi.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BaseSettings _baseSettings;
    private readonly EmailHelper _emailHelper;

    public AccountService(
        IUnitOfWork unitOfWork,
        EmailHelper emailHelper,
        IOptions<BaseSettings> options
    )
    {
        _unitOfWork = unitOfWork;
        _emailHelper = emailHelper;
        _baseSettings = options.Value;
    }

    public async Task<(List<AccountNameResponse>, DateTime?)> GetAccountByNameAsync(
        string name,
        DateTime? cursor,
        int pageSize
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return ([], null);

        var query = _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .Where(a =>
                (a.Username.Contains(name) || a.DisplayName.Contains(name))
                && (cursor == null || a.CreatedAt < cursor)
            )
            .OrderByDescending(a => a.CreatedAt)
            .Take(pageSize + 1);

        var accounts = await query
            .Select(a => new AccountNameResponse
            {
                Id = a.Id,
                Username = a.Username,
                DisplayName = a.DisplayName,
                Avatar = a.Picture != null ? a.Picture.Link : string.Empty,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync();

        var hasMore = accounts.Count > pageSize;
        var nextCursor = hasMore ? accounts.Last().CreatedAt : (DateTime?)null;
        var result = accounts.Take(pageSize).ToList();

        return (result, nextCursor);
    }

    public async Task<AccountResponse?> GetProfileByIdAsync(Guid accountId)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .Where(x => x.Id == accountId)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                IsOwner = a.Id == accountId,
                Language = a.Language.ToString(),
                DisplayName = a.DisplayName,
                DateOfBirth = a.DateOfBirth,
                AvatarUrl = a.Picture != null ? a.Picture.Link : "",
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync();

        return account;
    }

    public async Task<AccountResponse?> GetProfileByUsernameAsync(string username, Guid userId)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .Where(x => x.Username == username)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                IsOwner = a.Id == userId,
                DisplayName = a.DisplayName,
                DateOfBirth = a.DateOfBirth,
                AvatarUrl = a.Picture != null ? a.Picture.Link : "",
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync();

        return account;
    }

    public async Task<AccountResponse?> UpdateAccountAsync(
        Guid accountId,
        UpdateAccountRequest request
    )
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null);

        if (account == null)
        {
            return null;
        }

        var updateTime = DateTime.UtcNow;
        account.Username = request.Username ?? account.Username;
        account.DisplayName = request.DisplayName ?? account.DisplayName;
        account.DateOfBirth = request.DateOfBirth ?? account.DateOfBirth;
        account.UpdatedAt = updateTime;

        await _unitOfWork.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            DisplayName = account.DisplayName,
            DateOfBirth = account.DateOfBirth,
            AvatarUrl = account.Picture != null ? account.Picture.Link : "",
            IsOwner = true,
            CreatedAt = account.CreatedAt,
            Email = account.Email,
            Status = account.Status.ToString(),
            Language = account.Language.ToString(),
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid accountId, string password)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null);

        if (account == null)
        {
            return false;
        }

        account.HashedPassword = PasswordHasherHelper.HashPassword(password);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsPasswordCorrectAsync(Guid accountId, string password)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null);

        return account != null
            && PasswordHasherHelper.VerifyPassword(password, account.HashedPassword);
    }

    public async Task<bool> ChangeAvatarAsync(Guid accountId, string avatarFile)
    {
        //Check if account exists
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .Include(a => a.Picture)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null);

        if (account == null)
        {
            return false;
        }

        var picture = await _unitOfWork
            .Pictures.GetQuery()
            .FirstOrDefaultAsync(p => p.Link == avatarFile);

        if (picture == null)
        {
            return false;
        }

        picture.AccountId = accountId;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DateTime?> SelfRemoveAccount(Guid accountId)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .AsNoTracking()
            .Include(a => a.Picture)
            .FirstOrDefaultAsync(a =>
                a.Id == accountId && a.SelfRemoveTime == null && a.DeletedAt == null
            );

        if (account == null)
        {
            return null;
        }

        account.SelfRemoveTime = DateTime.UtcNow.AddDays(_baseSettings.SelfRemoveDurationDays);

        await _emailHelper.SendAccountRemovalEmailAsync(account.Email);

        await _unitOfWork.SaveChangesAsync();

        return account.SelfRemoveTime;
    }
}
