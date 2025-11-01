using System;
using BusinessObject.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByUsernameAsync(string username);
    Task<Account?> GetByEmailAsync(string email);
    Task<List<Account>> GetByNameAsync(string name);
    Task<Account?> GetByRefreshTokenAsync(string refreshToken);
    Task<Account?> GetByConfirmCodeAsync(string confirmCode);
}
