using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.CreateService;

namespace ServicesAPI.Endpoints.UpdateCategory;

public record UpdateCategoryCommand(long Id, string CategoryName, TimeSpan TimeSlotSize) : ICommand;

public class UpdateCategoryCommandHandler(ServicesDbContext context) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await context.ServiceCategories.FindAsync([command.Id], ct);

        if (category == null)
        {
            return CategoryErrors.CategoryNotFound();
        }

        try
        {
            category.CategoryName = command.CategoryName;
            category.TimeSlotSize = command.TimeSlotSize;

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
