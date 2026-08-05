using Deunde.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Deunde.IdentityServer.Pages.Account.ConfirmEmailCallback;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly IEmailVerificationManager _verificationManager;

    public Index(
        IEmailVerificationManager verificationManager)
    {
        _verificationManager = verificationManager;
    }
    [TempData]
    public string? StatusMessage { get; set; }
    public bool IsSucceeded { get; set; }
    public string? ReturnUrl { get; set; }
    
    public async Task<IActionResult> OnGetAsync(string? userId, string? token, string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "~/";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            StatusMessage = "Error: Invalid email confirmation parameters.";
            IsSucceeded = false;
            return Page();
        }

        var result = await _verificationManager.ConfirmEmailAsync(userId, token);

        if (result.Succeeded)
        {
            StatusMessage = "Thank you! Your email has been successfully confirmed.";
            IsSucceeded = true;
        }
        else
        {
            StatusMessage = result.Errors.FirstOrDefault()?.Description 
                            ?? "Email confirmation failed. The link may have expired or already been used.";
            IsSucceeded = false;
        }

        return Page();
    }
}