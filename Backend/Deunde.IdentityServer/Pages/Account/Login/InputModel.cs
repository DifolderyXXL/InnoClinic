using System.ComponentModel.DataAnnotations;

namespace Deunde.IdentityServer.Pages.Login;

public class InputModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    public bool RememberLogin { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }
}
