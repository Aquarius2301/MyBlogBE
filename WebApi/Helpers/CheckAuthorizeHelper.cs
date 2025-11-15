using System;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Dtos;

namespace WebApi.Helpers;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
/// <summary>
/// Authorization filter to check user status before allowing access to the endpoint.
/// </summary>
public class CheckStatusHelper : Attribute, IAsyncAuthorizationFilter
{
    private readonly StatusType[] _requiredStatus;

    public CheckStatusHelper(StatusType requiredStatus)
    {
        _requiredStatus = [requiredStatus];
    }

    public CheckStatusHelper(StatusType[] requiredStatus)
    {
        _requiredStatus = requiredStatus;
    }

    /// <summary>
    /// Checks the user's status and authorizes access accordingly.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = ApiResponse.Unauthorized();
            return;
        }

        var claimValue = user.Claims.FirstOrDefault(c => c.Type == "Status")?.Value;

        if (!int.TryParse(claimValue, out int statusInt))
        {
            context.Result = ApiResponse.Forbidden();
            return;
        }

        var userStatus = (StatusType)statusInt;

        if (!_requiredStatus.Contains(userStatus))
        {
            context.Result = ApiResponse.Forbidden();
            return;
        }

        await Task.CompletedTask;
    }
}
