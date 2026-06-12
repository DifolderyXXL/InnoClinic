using System;
using Microsoft.AspNetCore.Identity;

namespace AuthorizationAPI.Services;

public interface IConfirmationSender
{
    public Task SendConfirmation(IdentityUser user, string token, bool useCookies, bool rememberMe);
}
