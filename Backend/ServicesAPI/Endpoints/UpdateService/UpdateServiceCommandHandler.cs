using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.UpdateService;

public record UpdateServiceCommand(long Id, string ServiceName, decimal Price, bool IsActive, long CategoryId, long SpecializationId) : ICommand;

public class UpdateServiceCommandHandler(ServicesDbContext context) : ICommandHandler<UpdateServiceCommand>
{
    public async Task<Result> Handle(UpdateServiceCommand command, CancellationToken ct)
    {
        var service = await context.Services.FindAsync([command.Id], ct);

        if (service == null)
        {
            return Result.Failure(new Error("Service not found", ErrorType.NotFound));
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

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
