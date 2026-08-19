using Contracts.ProfilesContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using ProfilesAPI.Application;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.User.DeleteUserAccountById;

public class DeleteUserAccountByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("users/{userId:guid}", async (
            Guid userId,
            ICommandHandler<DeleteUserAccountByIdCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(userId), ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Accounts.Manage);
    }
}

public record DeleteUserAccountByIdCommand(Guid UserId) : ICommand;

public class DeleteUserAccountByIdCommandHandler(
    ProfilesDbContext context,
    IIdentityServiceClient identityService,
    IUserAccountCleaner cleaner,
    IPublishEndpoint publishEndpoint) : ICommandHandler<DeleteUserAccountByIdCommand>
{
    public async Task<Result> Handle(DeleteUserAccountByIdCommand command, CancellationToken ct)
    {
        var identityResult = await identityService.DeleteIdentityUserAsync(command.UserId, ct);
        if (identityResult.IsError)
        {
            return identityResult.Error!;
        }

        var cleanerResult = await cleaner.DeleteUserProfilesAndAccount(command.UserId, ct);
        if (cleanerResult.IsError)
        {
            return cleanerResult.Error!;
        }

        await publishEndpoint.Publish(new UserDeletionRequestedIntegrationEvent(command.UserId, DateTime.UtcNow), ct);

        await context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}