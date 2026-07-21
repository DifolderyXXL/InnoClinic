using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Specializations.DeleteSpecialization;

namespace ServicesAPI.Endpoints.Specializations.DeleteSpecialization;

public record DeleteSpecializationCommand(long Id) : ICommand;

public class DeleteSpecializationCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<DeleteSpecializationCommand>
{
    public async Task<Result> Handle(DeleteSpecializationCommand command, CancellationToken ct)
    {
        var specialization = await context.Specializations.FindAsync([command.Id], ct);

        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }

        try
        {
            context.Specializations.Remove(specialization);

            await publishEndpoint.Publish(new SpecializationDeletedEvent()
            {
                SpecializationName = specialization.SpecializationName,
                IsActive = specialization.IsActive,
                Id = specialization.Id
            }, ct);

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
