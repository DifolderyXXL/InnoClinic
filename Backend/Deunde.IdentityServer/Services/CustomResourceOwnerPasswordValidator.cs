using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IRoleResolver _roleResolver;

    public CustomResourceOwnerPasswordValidator(UserManager<IdentityUser> userManager, IRoleResolver roleResolver)
    {
        _userManager = userManager;
        _roleResolver = roleResolver;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userManager.FindByEmailAsync(context.UserName) 
                   ?? await _userManager.FindByNameAsync(context.UserName);

        if (user != null && await _userManager.CheckPasswordAsync(user, context.Password))
        {
            var acrValue = context.Request.Raw?.Get("acr_values")?.Split(" ");
            var roleClaim = await _roleResolver.ResolveUserRoleClaimAsync(user, acrValue ==null ? [] :[..acrValue]);

            context.Result = new GrantValidationResult(
                subject: user.Id,
                authenticationMethod: "password",
                claims: [roleClaim]
            );

            return;
        }

        context.Result = new GrantValidationResult(
            Duende.IdentityServer.Models.TokenRequestErrors.InvalidGrant, 
            "Invalid credentials"
        );
    }
}