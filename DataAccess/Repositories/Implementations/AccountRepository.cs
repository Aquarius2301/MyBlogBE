using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

/// <summary>
/// Repository for managing <see cref="Account"/> entities.
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(MyBlogContext context)
        : base(context) { }

    public new async Task<ICollection<Account>> GetAllAsync(bool includeDeleted = false)
    {
        var accounts = await _context
            .Accounts.Where(a => includeDeleted || a.DeletedAt == null)
            .ToListAsync();

        return accounts;
    }

    public new async Task<Account?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.Id == id && (includeDeleted || a.DeletedAt == null)
        );

        return account;
    }

    public async Task<ICollection<Account>> GetAllAsync(
        bool includeInactive = false,
        bool includeDeleted = false
    )
    {
        var accounts = await _context
            .Accounts.Where(a =>
                (includeInactive || a.Status != StatusType.InActive)
                && (includeDeleted || a.DeletedAt == null)
            )
            .ToListAsync();

        return accounts;
    }

    public async Task<Account?> GetByIdAsync(
        Guid id,
        bool includeDeleted = false,
        bool includeInactive = false
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.Id == id
            && (includeInactive || a.Status != StatusType.InActive)
            && (includeDeleted || a.DeletedAt == null)
        );

        return account;
    }

    public async Task<Account?> GetByUsernameAsync(
        string username,
        bool includeDeleted = false,
        bool includeInactive = false
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.Username == username
            && (includeInactive || a.Status != StatusType.InActive)
            && (includeDeleted || a.DeletedAt == null)
        );

        return account;
    }

    public async Task<Account?> GetByEmailAsync(
        string username,
        bool includeDeleted = false,
        bool includeInactive = false
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.Email == username
            && (includeInactive || a.Status != StatusType.InActive)
            && (includeDeleted || a.DeletedAt == null)
        );

        return account;
    }

    public async Task<Account?> GetByUsernameOrEmailAsync(
        string usernameOrEmail,
        bool includeDeleted = false,
        bool includeInactive = false
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            (a.Username == usernameOrEmail || a.Email == usernameOrEmail)
            && (includeInactive || a.Status != StatusType.InActive)
            && (includeDeleted || a.DeletedAt == null)
        );

        return account;
    }

    public async Task<Account?> GetByRefreshTokenAsync(
        string refreshToken,
        bool includeInactive = false
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.RefreshToken == refreshToken
            && (includeInactive || a.Status != StatusType.InActive)
            && a.DeletedAt == null
        );

        return account;
    }

    public async Task<ICollection<Account>> GetByNameAsync(
        string name,
        bool includeDeleted = false,
        bool includeInactive = false
    )
    {
        var accounts = await _context
            .Accounts.Where(a =>
                (a.DisplayName.Contains(name) || a.Username.Contains(name))
                && (includeInactive || a.Status != StatusType.InActive)
                && (includeDeleted || a.DeletedAt == null)
            )
            .ToListAsync();

        return accounts;
    }

    public async Task<Account?> GetByEmailVerifiedCode(
        string confirmCode,
        VerificationType verificationType
    )
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.EmailVerifiedCode == confirmCode
            && a.VerificationType == verificationType
            && a.DeletedAt == null
        );

        return account;
    }
}
