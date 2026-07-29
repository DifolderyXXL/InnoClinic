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
    public class RoleRequest
    {
        public string UserId { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/role", async ([FromBody] RoleRequest request, ICommandHandler<AssignUserRoleCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(request.UserId, request.Role), ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.IdentityServer);

        builder.MapDelete("/role", async ([FromBody] RoleRequest request, ICommandHandler<RemoveUserRoleCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(request.UserId, request.Role), ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}
public record AssignUserRoleCommand(string UserId, string Role) : ICommand;

public class AssignUserRoleCommandHandler(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : ICommandHandler<AssignUserRoleCommand>
{
    public async Task<Result> Handle(AssignUserRoleCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
        {
            return UserErrors.UserNotFound();
        }

        if (!await roleManager.RoleExistsAsync(command.Role))
        {
            return UserErrors.RoleNotFound();
        }

        if (await userManager.IsInRoleAsync(user, command.Role))
        {
            return Result.Success();
        }

        var result = await userManager.AddToRoleAsync(user, command.Role);

        return result.Succeeded ? Result.Success() : UserErrors.ErrorAddingRole();
    }
}

public record RemoveUserRoleCommand(string UserId, string Role) : ICommand;

public class RemoveUserRoleCommandHandler(UserManager<IdentityUser> userManager) : ICommandHandler<RemoveUserRoleCommand>
{
    public async Task<Result> Handle(RemoveUserRoleCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null)
        {
            return UserErrors.UserNotFound();
        }

        if (!await userManager.IsInRoleAsync(user, command.Role))
        {
            return Result.Success();
        }

        var result = await userManager.RemoveFromRoleAsync(user, command.Role);

        return result.Succeeded ? Result.Success() : UserErrors.ErrorRemovingRole();
    }
}
public static class UserErrors
{
    public static Error UserNotFound() => Error.Create(ErrorType.NotFound);
    public static Error RoleNotFound() => Error.Create(ErrorType.NotFound);
    public static Error ErrorAddingRole() => Error.Create(ErrorType.Problem);
    public static Error ErrorRemovingRole() => Error.Create(ErrorType.Problem);
}