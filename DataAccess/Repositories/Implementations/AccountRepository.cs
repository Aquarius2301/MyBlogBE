using System;
using BusinessObject;
using BusinessObject.Enums;
using BusinessObject.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(MyBlogContext context) : base(context)
    {
    }

    public Task<List<Account>> GetByNameAsync(string name)
    {
        return _context.Accounts
                .Where(x => x.Username == name || x.DisplayName == name
                    && x.DeletedAt == null).ToListAsync();
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Username == username && x.DeletedAt == null);
    }
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);
    }

    public async Task<Account?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Accounts
                    .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.DeletedAt == null);
    }

    public Task<Account?> GetByConfirmCodeAsync(string confirmCode)
    {
        return _context.Accounts
                .FirstOrDefaultAsync(x => x.EmailVerifiedCode == confirmCode && x.DeletedAt == null);
    }
}
