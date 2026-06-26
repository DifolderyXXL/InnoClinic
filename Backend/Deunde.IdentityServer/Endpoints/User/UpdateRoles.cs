using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MicroserviceApiKernel.Extensions;
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
        builder.MapPut("/api/role", async ([FromBody] Request request, ICommandHandler<ChangeUserRoleCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(request.UserId, request.Roles), ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}

public record ChangeUserRoleCommand(string UserId, string[] Roles) : ICommand;
public class ChangeUserRoleCommandHandler(UserManager<IdentityUser> userManager) : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<Result> Handle(ChangeUserRoleCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.UserNotFound();
        }

        var roles = await userManager.GetRolesAsync(user);

        var intersect = roles.Intersect(command.Roles);

        var r1 = await userManager.AddToRolesAsync(user, command.Roles.Except(intersect));
        var r2 = await userManager.RemoveFromRolesAsync(user, roles.Except(intersect));

        return (r1.Succeeded && r2.Succeeded) ? Result.Success() : UserErrors.ErrorAddingRole();
    }
}

public static class UserErrors
{
    public static Error UserNotFound() => Error.Create(ErrorType.NotFound);
    public static Error ErrorAddingRole() => Error.Create(ErrorType.Problem);
}