using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;

namespace OfficesApi.Endpoints.ChangeOfficeActive;

public record ChangeOfficeActiveCommand(string Id, bool IsActive) : ICommand;

public class ChangeOfficeActiveCommandHandler(OfficesDbContext context) : ICommandHandler<ChangeOfficeActiveCommand>
{
    public async Task<Result> Handle(ChangeOfficeActiveCommand command, CancellationToken ct)
    {
        var officeResult = await context.GetOffice(command.Id, ct);

        if (officeResult.IsError)
        {
            return officeResult;
        }

        var office = officeResult.Value!;

        var result = await context.UpdateOfficeActive(office, command.IsActive, ct);

        return result;
    }
}
