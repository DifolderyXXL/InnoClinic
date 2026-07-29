using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public class RoleEnsuranceProfileService(
    UserManager<IdentityUser> userManager) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.FindByIdAsync(context.Subject.GetSubjectId());

        if (user != null)
        {
            context.AddRequestedClaims(context.Subject.Claims);

            var claims = await userManager.GetClaimsAsync(user);
            context.AddRequestedClaims(claims);

            if (context.RequestedClaimTypes.Contains(JwtClaimTypes.Email) && !string.IsNullOrEmpty(user.Email))
            {
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Email, user.Email));
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false"));
            }

            var selectedRoleClaim = context.Subject.Claims.FirstOrDefault(c => 
                c.Type == JwtClaimTypes.Role || 
                c.Type == ClaimTypes.Role || 
                c.Type == "role");

            if (selectedRoleClaim != null)
            {
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, selectedRoleClaim.Value));
            }
        }
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.FindFirst(JwtClaimTypes.Subject)?.Value;
        if (!string.IsNullOrEmpty(sub))
        {
            var user = await userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}