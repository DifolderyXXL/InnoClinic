using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.UpdateSpecialization;

public record UpdateSpecializationCommand(long Id, string SpecializationName, bool IsActive) : ICommand;

public class UpdateSpecializationCommandHandler(ServicesDbContext context) : ICommandHandler<UpdateSpecializationCommand>
{
    public async Task<Result> Handle(UpdateSpecializationCommand command, CancellationToken ct)
    {
        var specialization = await context.Specializations.FindAsync([command.Id], ct);

        if (specialization == null)
        {
            return Result.Failure(new Error("Specialization not found", ErrorType.NotFound));
        }

        try
        {
            specialization.SpecializationName = command.SpecializationName;
            specialization.IsActive = command.IsActive;

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, ErrorType.Internal));
        }

        return Result.Success();
    }
}
