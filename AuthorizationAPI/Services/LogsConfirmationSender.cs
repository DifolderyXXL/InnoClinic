using Microsoft.AspNetCore.Identity;

namespace AuthorizationAPI.Services;

public class LogsConfirmationSender(IHttpContextAccessor contextAccessor, ILogger<LogsConfirmationSender> logger) : IConfirmationSender
{
    public Task SendConfirmation(IdentityUser user, string token, bool useCookies, bool rememberMe)
    {
        var context = contextAccessor.HttpContext;
        if(context == null)
            return Task.CompletedTask;

        string scheme = context.Request.Scheme;
        string host = context.Request.Host.Value;

        string baseHostLink = $"{scheme}://{host}";

        string approveLink = $"{baseHostLink}/core/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}&useCookies={useCookies}&rememberMe={rememberMe}";

        logger.LogInformation($"Email confirmation link for user {user.Email}: {approveLink}");

        return Task.CompletedTask;
    }
}