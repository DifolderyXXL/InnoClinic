using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Deunde.IdentityServer.Pages.Account.SetPassword;

[AllowAnonymous]
public class ConfirmationModel : PageModel
{
    public void OnGet()
    {
    }
}