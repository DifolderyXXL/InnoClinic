using Microsoft.AspNetCore.Identity;

namespace AuthorizationAPI.Services;

public interface ISessionAuthorizeManager
{
    public Task<bool> AuthorizeSession(IdentityUser user, bool useCookies, bool rememberMe);
}
