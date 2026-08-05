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


    public IActionResult OnGet(string email, string returnUrl)
    {
        if (string.IsNullOrEmpty(Input.Email))
        {
            return RedirectToPage("/Index");
        }
        
        return Page();
    }
}