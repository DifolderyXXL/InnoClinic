using System;
using MicroserviceApiKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deunde.IdentityServer.Endpoints.User;

public class UpdateRoles : IEndpoint
{
    class Request
    {
        public string UserId;
        public string[] Roles;
    }
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/api/role", async ([FromBody] Request request, UserManager<IdentityUser> userManager, CancellationToken ct) =>
        {
            var user = await userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return Results.BadRequest();
            }

            var roles = await userManager.GetRolesAsync(user);

            var intersect = roles.Intersect(request.Roles);

            var r1 = await userManager.AddToRolesAsync(user, request.Roles.Except(intersect));
            var r2 = await userManager.RemoveFromRolesAsync(user, roles.Except(intersect));

            return (r1.Succeeded && r2.Succeeded) ? Results.Created() : Results.Problem();
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}
