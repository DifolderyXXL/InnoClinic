using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Services.CreateService;

namespace ServicesAPI.Endpoints.Specializations.UpdateSpecialization;

public record UpdateSpecializationCommand(long Id, string SpecializationName, bool IsActive) : ICommand;

public class UpdateSpecializationCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<UpdateSpecializationCommand>
{
    public async Task<Result> Handle(UpdateSpecializationCommand command, CancellationToken ct)
    {
        var specialization = await context.Specializations.FindAsync([command.Id], ct);

        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }

        try
        {
            specialization.SpecializationName = command.SpecializationName;
            specialization.IsActive = command.IsActive;

            await publishEndpoint.Publish(new SpecializationUpdatedEvent
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

