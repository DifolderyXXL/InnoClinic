using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.Categories.CreateCategory;

public record CreateCategoryCommand(string CategoryName, TimeSpan TimeSlotSize) : ICommand;

public class CreateCategoryCommandHandler(ServicesDbContext context) : ICommandHandler<CreateCategoryCommand>
{
    public async Task<Result> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        try
        {
            var category = new ServiceCategory
            {
                CategoryName = command.CategoryName,
                TimeSlotSize = command.TimeSlotSize
            };

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
