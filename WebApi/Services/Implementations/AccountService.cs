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
        var account =
            await _unitOfWork.Accounts.GetByIdAsync(accountId)
            ?? throw new Exception("Account not found"); // User can be deleted

        account.Username = request.Username ?? account.Username;
        account.DisplayName = request.DisplayName ?? account.DisplayName;
        account.DateOfBirth = request.DateOfBirth ?? account.DateOfBirth;

        await _unitOfWork.SaveChangesAsync();

        return new UpdateAccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            DisplayName = account.DisplayName,
            DateOfBirth = account.DateOfBirth,
        };
    }

    public async Task ChangePasswordAsync(Guid accountId, UpdatePasswordRequest request)
    {
        var account =
            await _unitOfWork.Accounts.GetByIdAsync(accountId)
            ?? throw new Exception("Account not found"); // User can be deleted

        account.HashedPassword = PasswordHasherHelper.HashPassword(request.NewPassword);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsPasswordCorrectAsync(Guid accountId, string password)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);

        return account != null
            && PasswordHasherHelper.VerifyPassword(password, account.HashedPassword);
    }

    public async Task<ImageDto> ChangeAvatarAsync(Guid accountId, IFormFile avatarFile)
    {
        var imageDto = _cloudinaryHelper.Upload(avatarFile);

        var picture = await _unitOfWork.Pictures.GetByAccountIdAsync(accountId);

        if (picture != null)
        {
            picture.Link = imageDto.Link;
            picture.PublicId = imageDto.PublicId;
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            var avatar = new Picture
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Link = imageDto.Link,
                PublicId = imageDto.PublicId,
            };

            _unitOfWork.Pictures.Add(avatar);
            await _unitOfWork.SaveChangesAsync();
        }

        return imageDto;
    }

    public async Task SelfRemoveAccount(Guid accountId)
    {
        var account =
            await _unitOfWork.Accounts.GetByIdAsync(accountId)
            ?? throw new Exception("Account not found"); // User can be deleted

        account.SelfRemoveTime = DateTime.UtcNow.AddDays(_baseSettings.SelfRemoveDurationDays);

        await _emailHelper.SendAccountRemovalEmailAsync(account.Email);

        await _unitOfWork.SaveChangesAsync();
    }
}
