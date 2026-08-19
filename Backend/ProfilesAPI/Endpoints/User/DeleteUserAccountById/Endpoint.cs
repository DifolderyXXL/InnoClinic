using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using ProfilesAPI.Application;

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
    IDocumentsApiServiceClient documents, 
    IAppointmentsApiServiceClient appointments,
    IUserAccountCleaner cleaner) : ICommandHandler<DeleteUserAccountByIdCommand>
{
    public async Task<Result> Handle(DeleteUserAccountByIdCommand command, CancellationToken ct)
    {
        var photosTask = documents.DeleteAllUserPhotos(command.UserId, ct);
        var medicalResultsTask = documents.DeleteAllUserMedicalResults(command.UserId, ct);
        var appointmentResultTask = appointments.DeleteAllUserAppointments(command.UserId, ct);
        var cleanerResultTask = cleaner.DeleteUserProfilesAndAccount(command.UserId, ct);
        
        var results = await Task.WhenAll(photosTask, medicalResultsTask, appointmentResultTask, cleanerResultTask);
        
        var failedResult = results.FirstOrDefault(r => r.IsError);
        if (failedResult is not null)
        {
            return failedResult.Error!;
        }

        return Result.Success();
    }
}