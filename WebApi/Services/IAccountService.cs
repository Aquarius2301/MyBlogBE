using System;
using WebApi.Dtos;

namespace WebApi.Services;

public interface IAccountService
{
    /// <summary>
    /// Get profile information by account ID
    /// </summary>
    /// <param name="accountId">The unique identifier of the account </param>
    /// <returns> The profile information of the account, or null if not found </returns>
    Task<AccountResponse?> GetProfileByIdAsync(Guid accountId);

    /// <summary>
    /// Get accounts by name with pagination
    /// </summary>
    /// <param name="name">The name to search for (username or display name)</param>
    /// <param name="cursor">The cursor for pagination (created at date of the last
    /// item from the previous page)</param>
    /// <param name="pageSize">The number of items to return per page</param>
    /// <returns> A list of accounts matching the name </returns>
    Task<List<AccountNameResponse>> GetAccountByNameAsync(
        string name,
        DateTime? cursor,
        int pageSize
    );

    /// <summary>
    /// Update account information
    /// </summary>
    /// <param name="accountId">The unique identifier of the account</param>
    /// <param name="request">The update account request data</param>
    /// <returns>The updated account information</returns>
    Task<UpdateAccountResponse> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request);

    /// <summary>
    /// Change account password
    /// </summary>
    /// <param name="accountId">The unique identifier of the account</param>
    /// <param name="request">The update password request data</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ChangePasswordAsync(Guid accountId, UpdatePasswordRequest request);

    /// <summary>
    /// Check if the provided password is correct for the account
    /// </summary>
    /// <param name="accountId">The unique identifier of the account</param>
    /// <param name="password">The password to verify</param>
    /// <returns>True if the password is correct, false otherwise</returns>
    Task<bool> IsPasswordCorrectAsync(Guid accountId, string password);

    /// <summary>
    /// Change account avatar
    /// </summary>
    /// <param name="accountId">The unique identifier of the account</param>
    /// <param name="avatarFile">The new avatar file</param>
    /// <returns>The updated image data</returns>
    Task<ImageDto> ChangeAvatarAsync(Guid accountId, IFormFile avatarFile);

    /// <summary>
    /// Schedule account for self-removal
    /// </summary>
    /// <param name="accountId">The unique identifier of the account</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SelfRemoveAccount(Guid accountId);
}
