using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Categories;
using ServicesAPI.Endpoints.Services.DeleteService;
using ServicesAPI.Endpoints.Specializations;

namespace ServicesAPI.Endpoints.Services.UpdateService;

public record UpdateServiceCommand(long Id, string ServiceName, decimal Price, bool IsActive, long CategoryId, long SpecializationId) : ICommand;

public class UpdateServiceCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<UpdateServiceCommand>
{
    public async Task<Result> Handle(UpdateServiceCommand command, CancellationToken ct)
    {
        var service = await context.Services.FindAsync([command.Id], ct);

        if (service == null)
        {
            return ServiceErrors.ServiceNotFound();
        }

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

        try
        {
            service.ServiceName = command.ServiceName;
            service.Price = command.Price;
            service.IsActive = command.IsActive;
            service.CategoryId = command.CategoryId;
            service.SpecializationId = command.SpecializationId;

            await publishEndpoint.Publish(new ServiceUpdatedEvent
            {
                Id = service.Id,
                CategoryId = service.CategoryId,
                ServiceName = service.ServiceName,
                Price = service.Price,
                SpecializationId = service.SpecializationId,
                IsActive = service.IsActive,
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