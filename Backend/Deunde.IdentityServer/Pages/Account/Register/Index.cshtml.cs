using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Deunde.IdentityServer.Extensions;
using Deunde.IdentityServer.Services;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Pages.Account.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserCreateManager _userCreateManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<IdentityUser> userManager,
        IUserCreateManager userCreateManager)
    {
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
        _userManager = userManager;
        _userCreateManager = userCreateManager;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        if (Input.Button != "register")
        {
            if (context != null)
            {
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                {
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }
            else
            {
                return Redirect("~/");
            }
        }

        if (ModelState.IsValid)
        {
            var existingUser = await _userManager.FindByEmailAsync(Input.Email!);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                await BuildModelAsync(Input.ReturnUrl);
                return Page();
            }

            bool isEmailVerificationRequired = _userManager.Options.SignIn.RequireConfirmedEmail;
            bool defaultEmailConfirmed = !isEmailVerificationRequired;
            var (newUser, result) = await _userCreateManager.CreateInternal(
                Input.Email!, 
                Input.Password!, 
                [UserCreateManager.ClientRole]
            );

            if (result.Succeeded)
            {
                await _events.RaiseAsync(new UserLoginSuccessEvent(newUser.UserName, newUser.Id, newUser.UserName, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLogin(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider);

                var props = new AuthenticationProperties();
                if (LoginOptions.AllowRememberLogin && Input.RememberLogin)
                {
                    props.IsPersistent = true;
                    props.ExpiresUtc = DateTimeOffset.UtcNow.Add(LoginOptions.RememberMeLoginDuration);
                }
                
                var additionalClaims = new List<Claim>
                {
                    new(JwtClaimTypes.Role, UserCreateManager.ClientRole)
                };

                var isuser = new IdentityServerUser(newUser.Id)
                {
                    DisplayName = newUser.UserName,
                    AdditionalClaims = additionalClaims
                };

                await HttpContext.SignInAsync(isuser, props);

                if (context != null)
                {
                    ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                    if (context.IsNativeClient())
                    {
                        return this.LoadingPage(Input.ReturnUrl);
                    }

                    return Redirect(Input.ReturnUrl ?? "~/");
                }

                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    throw new ArgumentException("invalid return URL");
                }
            }

            foreach (var errorItem in result.Errors)
            {
                ModelState.AddModelError(string.Empty, errorItem.Description);
            }
        }

        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Name,
                displayName: x.DisplayName ?? x.Name
            )).ToList();

        var allowLocal = true;
        var client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = [.. providers]
        };
    }
}