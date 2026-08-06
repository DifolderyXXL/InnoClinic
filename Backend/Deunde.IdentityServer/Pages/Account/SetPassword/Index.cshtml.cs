using System.Text;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Deunde.IdentityServer.Pages.Account.SetPassword;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;

    public Index(
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        IIdentityServerInteractionService interaction)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
    }
    
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public async Task<IActionResult> OnGetAsync(string userId, string token, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid request: missing required token or userId.");
        }

        Input = new InputModel
        {
            UserId = userId,
            Token = token,
            ReturnUrl = returnUrl
        };

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Input.UserId);
        if (user == null)
        {
            return RedirectToPage("/Account/SetPassword/Confirmation");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Invalid or corrupted password token.");
            return Page();
        }
        
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);

        if (result.Succeeded)
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            if (!string.IsNullOrEmpty(Input.ReturnUrl))
            {
                if (Url.IsLocalUrl(Input.ReturnUrl) || _interaction.IsValidReturnUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
            }

            return Redirect("~/");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}