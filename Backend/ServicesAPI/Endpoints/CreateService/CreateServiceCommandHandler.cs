using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.CreateService;

public record CreateServiceCommand(string ServiceName, decimal Price, bool IsActive, long CategoryId, long SpecializationId) : ICommand;

public class CreateServiceCommandHandler(ServicesDbContext context) : ICommandHandler<CreateServiceCommand>
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

        try
        {
            await context.Services.AddAsync(new()
            {
                ServiceName = command.ServiceName,
                Price = command.Price,
                IsActive = command.IsActive,
                CategoryId = command.CategoryId,
                SpecializationId = command.SpecializationId
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
