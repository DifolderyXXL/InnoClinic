using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Deunde.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Deunde.IdentityServer.Services;

namespace Deunde.IdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserCreateManager _userCreateManager;

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
        UserManager<IdentityUser> userManager,
        IUserCreateManager userCreateManager,
        SignInManager<IdentityUser> signInManager)
    {
        _interaction = interaction;
        _logger = logger;
        _events = events;
        _userManager = userManager;
        _userCreateManager = userCreateManager;

        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result.Succeeded != true)
        {
            throw new InvalidOperationException($"External authentication error: {result.Failure}");
        }

        var externalUser = result.Principal ??
            throw new InvalidOperationException("External authentication produced a null Principal");

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.ExternalClaims(externalClaims);
        }

        var providerKey = externalUser.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     externalUser.FindFirst("sub")?.Value
                     ?? throw new InvalidOperationException("Unknown userid");
        var email = externalUser.FindFirst(ClaimTypes.Email)?.Value ??
                     externalUser.FindFirst("email")?.Value ??
                     externalUser.FindFirst("mail")?.Value
                     ?? throw new InvalidOperationException("Unknown email");


        var provider = result.Properties.Items["scheme"] ?? throw new InvalidOperationException("Null scheme in authentication properties");
        var providerDisplayName = externalUser.FindFirst(ClaimTypes.Name)?.Value ?? email;

        var user = await _userManager.FindByLoginAsync(provider, providerKey);

        var isExternalEmailVerified = externalUser.FindFirst("email_verified")?.Value == "true";

        if (user == null)
        {
            bool isEmailVerificationRequired = _userManager.Options.SignIn.RequireConfirmedEmail;
            bool defaultEmailConfirmed = !isEmailVerificationRequired;
            // find external user
            user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                //TODO: security leak, check providers

                // if (!isExternalEmailVerified)
                // {
                //     throw new InvalidOperationException("Automatic account linking failed because the external email provider has not verified this email address.");
                // }

                if (isEmailVerificationRequired && !user.EmailConfirmed)
                {
                    throw new InvalidOperationException("Automatic account linking failed because the external email is unverified.");
                }
            }
            else
            {
                user = await _userCreateManager.CreateClientExternal(email);
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, providerDisplayName));
            if (!addLoginResult.Succeeded)
            {
                throw new InvalidOperationException($"Can't bind external login: {addLoginResult.Errors.First().Description}");
            }
        }

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        var additionalClaims = new List<Claim>();
        var isuser = new IdentityServerUser(user.Id)
        {
            DisplayName = user.UserName,
            IdentityProvider = provider,
            AdditionalClaims = additionalClaims
        };


        await HttpContext.SignInAsync(isuser, localSignInProps);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerKey, user.Id, user.UserName, true, context?.Client.ClientId));
        Telemetry.Metrics.UserLogin(context?.Client.ClientId, provider!);

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage(returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private static void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        ArgumentNullException.ThrowIfNull(externalResult.Principal, nameof(externalResult.Principal));

        // capture the idp used to login, so the session knows where the user came from
        localClaims.Add(new Claim(JwtClaimTypes.IdentityProvider, externalResult.Properties?.Items["scheme"] ?? "unknown identity provider"));

        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties?.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}
