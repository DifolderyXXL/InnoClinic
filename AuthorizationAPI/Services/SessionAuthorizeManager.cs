using Microsoft.AspNetCore.Identity;

namespace AuthorizationAPI.Services;

public class SessionAuthorizeManager(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : ISessionAuthorizeManager
{
    public async Task<bool> AuthorizeSession(IdentityUser user, bool useCookies, bool rememberMe)
    {
        bool isEmailVerificationRequired = userManager.Options.SignIn.RequireConfirmedEmail;
        if (isEmailVerificationRequired)
        {
            var confirmed = await userManager.IsEmailConfirmedAsync(user);
            if (!confirmed)
            {
                return false;
            }
        }

        if (useCookies)
        {
            await signInManager.SignInAsync(user, isPersistent: rememberMe);
        }
        else
        {
            throw new NotImplementedException("Token-based authentication is not implemented yet.");
        }

        return true;
    }
}
