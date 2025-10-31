using System;
using BusinessObject;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Helpers;

public class AccountCleanupHelper : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public AccountCleanupHelper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredAccountsAsync();
            await SelfRemoveAccountsAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupExpiredAccountsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var expiredAccounts = await unitOfWork.Accounts.GetQuery()
            .Where(a => !string.IsNullOrEmpty(a.EmailVerifiedCode) && a.CreatedAt.AddMinutes(30) <= now)
            .ToListAsync();

        if (expiredAccounts.Any())
        {
            foreach (var account in expiredAccounts)
            {
                await unitOfWork.Accounts.DeleteAsync(account);
            }
            Console.WriteLine($"[Cleanup] Deleted {expiredAccounts.Count} expired unverified accounts at {now}.");
        }
    }

    private async Task SelfRemoveAccountsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var expiredAccounts = await unitOfWork.Accounts.GetQuery()
            .Where(a => a.SelfRemoveTime != null && a.SelfRemoveTime <= now)
            .ToListAsync();

        if (expiredAccounts.Any())
        {
            foreach (var account in expiredAccounts)
            {
                account.SelfRemoveTime = null;
                account.DeletedAt = now;
                await unitOfWork.Accounts.UpdateAsync(account);
            }
            Console.WriteLine($"[Cleanup] Soft-deleted {expiredAccounts.Count} accounts scheduled for self-removal at {now}.");
        }
    }
}
