using Contracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Categories.DeleteCategory;

namespace ServicesAPI.Endpoints.Categories.DeleteCategory;

public record DeleteCategoryCommand(long Id) : ICommand;

public class DeleteCategoryCommandHandler(ServicesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await context.ServiceCategories.FindAsync([command.Id], ct);

        if (category == null)
        {
            return CategoryErrors.CategoryNotFound();
        }

        try
        {
            context.ServiceCategories.Remove(category);
            await context.SaveChangesAsync(ct);
            
            await publishEndpoint.Publish(new CategoryDeletedEvent()
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                TimeSlotSize = category.TimeSlotSize
            }, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
