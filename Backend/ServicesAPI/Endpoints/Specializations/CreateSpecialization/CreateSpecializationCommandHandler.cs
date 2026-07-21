using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.Specializations.CreateSpecialization;

public record CreateSpecializationCommand(string SpecializationName, bool IsActive) : ICommand;

public class CreateSpecializationCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<CreateSpecializationCommand>
{
    public async Task<Result> Handle(CreateSpecializationCommand command, CancellationToken ct)
    {
        
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
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

            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
