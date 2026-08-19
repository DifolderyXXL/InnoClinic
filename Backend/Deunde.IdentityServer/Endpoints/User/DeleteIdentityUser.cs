using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deunde.IdentityServer.Endpoints.User;

public class DeleteIdentityUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/users/{userId}", async (
            [FromRoute] string userId,
            ICommandHandler<DeleteIdentityUserCommand> handler,
            CancellationToken ct) =>
        {
            var command = new DeleteIdentityUserCommand(userId);
            var result = await handler.Handle(command, ct);

            return result.MapToTypedResult(TypedResults.NoContent);
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}

public record DeleteIdentityUserCommand(string UserId) : ICommand;

public class DeleteIdentityUserCommandHandler(
    UserManager<IdentityUser> userManager,
    ILogger<DeleteIdentityUserCommandHandler> logger) : ICommandHandler<DeleteIdentityUserCommand>
{
    public async Task<Result> Handle(DeleteIdentityUserCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        
        if (user == null)
        {
            logger.LogInformation("Identity user with ID {UserId} was not found or already deleted.", command.UserId);
            return Result.Success();
        }

        await userManager.UpdateSecurityStampAsync(user);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to delete Identity user {UserId}. Errors: {Errors}", command.UserId, errors);

            return new Error($"Failed to delete user: {errors}", ErrorType.Internal);
        }

        logger.LogInformation("Successfully deleted Identity user with ID {UserId}.", command.UserId);

        return Result.Success();
    }
}

public class DeleteIdentityUserCommandValidator : AbstractValidator<DeleteIdentityUserCommand>
{
    public DeleteIdentityUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}