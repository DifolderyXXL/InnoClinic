using Contracts;
using MassTransit;
using MassTransit.Transports;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.CreateSpecialization;

public record CreateSpecializationCommand(string SpecializationName, bool IsActive) : ICommand;

public class CreateSpecializationCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<CreateSpecializationCommand>
{
    public async Task<Result> Handle(CreateSpecializationCommand command, CancellationToken ct)
    {
        try
        {
            var specialization = new Specialization
            {
                SpecializationName = command.SpecializationName,
                IsActive = command.IsActive
            };

            context.Specializations.Add(specialization);
            await context.SaveChangesAsync(ct);

            await publishEndpoint.Publish(new SpecializationCreatedEvent
            {
                SpecializationName = specialization.SpecializationName,
                IsActive = specialization.IsActive,
                Id = specialization.Id
            }, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
