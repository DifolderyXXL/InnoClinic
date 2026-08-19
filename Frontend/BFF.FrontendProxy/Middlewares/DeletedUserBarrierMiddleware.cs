using System.Security.Claims;
using BFF.FrontendProxy.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using ProfilesAPI.CustomBindAsync;

namespace BFF.FrontendProxy.Middlewares;

public class DeletedUserBarrierMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IRevokedUserRepository revokedUsers)
    {
        var userId = UserClaimInfo.GetUserId(context.User);
    
        if (!string.IsNullOrEmpty(userId))
        {
            var isRevoked = await revokedUsers.IsUserRevoked(userId, context.RequestAborted);
            if (isRevoked)
            {
                await context.SignOutAsync("cookie");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}