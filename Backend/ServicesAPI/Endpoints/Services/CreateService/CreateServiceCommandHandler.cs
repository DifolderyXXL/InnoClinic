using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Specializations;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.Services.CreateService;

public record CreateServiceCommand(string ServiceName, decimal Price, bool IsActive, long CategoryId, long SpecializationId) : ICommand;

public class CreateServiceCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<CreateServiceCommand>
{
    public async Task<Result> Handle(CreateServiceCommand command, CancellationToken ct)
    {
        var category = await context.ServiceCategories.FindAsync([command.CategoryId], ct);

        if (category == null)
        {
            return CategoryErrors.CategoryNotFound();
        }

        var specialization = await context.Specializations.FindAsync([command.SpecializationId], ct);

        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }

        
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var service = new Service
            {
                ServiceName = command.ServiceName,
                Price = command.Price,
                IsActive = command.IsActive,
                CategoryId = command.CategoryId,
                SpecializationId = command.SpecializationId,
            };
            await context.Services.AddAsync(service, ct);

            await context.SaveChangesAsync(ct);

            await publishEndpoint.Publish(new ServiceCreatedEvent
            {
                Id = service.Id,
                CategoryId = service.CategoryId,
                ServiceName = service.ServiceName,
                Price = service.Price,
                SpecializationId = service.SpecializationId,
                IsActive = service.IsActive,
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
