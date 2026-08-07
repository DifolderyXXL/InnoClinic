using System.ComponentModel.DataAnnotations;
using Deunde.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Deunde.IdentityServer.Pages.Account.ConfirmEmailNotice;

public class InputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}


[AllowAnonymous]
public class Index : PageModel
{
    [BindProperty(SupportsGet = true)]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }
    
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailVerificationManager _verificationManager;

    public Index(
        UserManager<IdentityUser> userManager, 
        IEmailVerificationManager verificationManager)
    {
        _userManager = userManager;
        _verificationManager = verificationManager;
    }
    public IActionResult OnGet(string email, string returnUrl)
    {
        if (string.IsNullOrEmpty(Input.Email))
        {
            return RedirectToPage("/Index");
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostResendAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user != null && !user.EmailConfirmed)
        {
            await _verificationManager.SendVerification(user, Input.ReturnUrl);
            StatusMessage = "Verification email has been resent. Please check your inbox.";
        }
        else
        {
            StatusMessage = "If an unverified account with this email exists, a new verification link has been sent.";
        }

        return Page();
    }
}