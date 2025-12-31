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
}
