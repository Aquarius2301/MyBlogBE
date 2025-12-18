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
    private readonly CloudinaryHelper _cloudinaryHelper;
    private readonly EmailHelper _emailHelper;

    public AccountService(
        IUnitOfWork unitOfWork,
        CloudinaryHelper cloudinaryHelper,
        EmailHelper emailHelper,
        IOptions<BaseSettings> options
    )
    {
        _unitOfWork = unitOfWork;
        _cloudinaryHelper = cloudinaryHelper;
        _emailHelper = emailHelper;
        _baseSettings = options.Value;
    }

    public async Task<List<AccountNameResponse>> GetAccountByNameAsync(
        string name,
        DateTime? cursor,
        int pageSize
    )
    {
        var account = await _unitOfWork
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
        var account = await _unitOfWork
            .Accounts.GetQuery()
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
            .FirstOrDefaultAsync(x => x.Id == accountId);

        return account;
    }

    public async Task<AccountResponse?> GetProfileByUsernameAsync(string username, Guid userId)
    {
        var account = await _unitOfWork
            .Accounts.GetQuery()
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
            .FirstOrDefaultAsync(x => x.Username == username);

        return account;
    }

    public async Task<UpdateAccountResponse?> UpdateAccountAsync(
        Guid accountId,
        UpdateAccountRequest request
    )
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

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

        return new UpdateAccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            DisplayName = account.DisplayName,
            DateOfBirth = account.DateOfBirth,
            UpdatedAt = updateTime,
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid accountId, string password)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

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
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

        return account != null
            && PasswordHasherHelper.VerifyPassword(password, account.HashedPassword);
    }

    public async Task<bool?> ChangeAvatarAsync(Guid accountId, string avatarFile)
    {
        //Check if account exists
        var account = await _unitOfWork
            .Accounts.GetQuery()
            .Include(a => a.Picture)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null);

        if (account == null)
        {
            return null;
        }

        var picture = await _unitOfWork.Pictures.GetQuery().FirstAsync(p => p.Link == avatarFile);

        account.Picture = picture;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DateTime?> SelfRemoveAccount(Guid accountId)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

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
