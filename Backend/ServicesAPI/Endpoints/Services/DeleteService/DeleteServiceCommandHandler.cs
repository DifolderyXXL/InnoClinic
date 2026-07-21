using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Services.DeleteService;

namespace ServicesAPI.Endpoints.Services.DeleteService;

public record DeleteServiceCommand(long Id) : ICommand;

public class DeleteServiceCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<DeleteServiceCommand>
{
    public async Task<Result> Handle(DeleteServiceCommand command, CancellationToken ct)
    {
        var service = await context.Services.FindAsync([command.Id], ct);

        if (service == null)
        {
            return ServiceErrors.ServiceNotFound();
        }

        try
        {
            context.Services.Remove(service);

            await publishEndpoint.Publish(new ServiceDeletedEvent
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
