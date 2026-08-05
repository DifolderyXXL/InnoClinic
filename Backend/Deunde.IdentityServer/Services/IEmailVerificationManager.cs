using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Deunde.IdentityServer.Services;

public interface IEmailVerificationManager
{
    public Task SendVerification(IdentityUser user, string returnUrl);
    Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
}

public class SmptEmailVerificationManager(
    IHttpContextAccessor contextAccessor,
    UserManager<IdentityUser> userManager,
    LinkGenerator linkGenerator,
    ILogger<SmptEmailVerificationManager> logger) : IEmailVerificationManager
{
    public async Task SendVerification(IdentityUser user, string returnUrl)
    {
        var context = contextAccessor.HttpContext;
        if (context == null)
        {
            logger.LogWarning("Cannot generate email verification link for user {UserId}: HttpContext is null.", user.Id);
            return;
        }

        var rawToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var approveLink = linkGenerator.GetUriByPage(
            httpContext: context,
            page: "/Account/ConfirmEmailCallback/Index",
            handler: null,
            values: new { userId = user.Id, token = encodedToken, returnUrl }
        );

        logger.LogInformation("Email confirmation link generated for user {Email}: {ConfirmLink}", user.Email, approveLink);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        if (user.EmailConfirmed)
        {
            return IdentityResult.Success;
        }
        try
        {
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                logger.LogInformation("Email successfully confirmed for user {Email}.", user.Email);
            }
            else
            {
                logger.LogWarning("Email confirmation failed for {Email}: {Errors}",
                    user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid token format during email confirmation for user ID {UserId}", userId);
            return IdentityResult.Failed(new IdentityError { Description = "Invalid confirmation token format." });
        }
    }
}