using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Receptionists.Create;

namespace ProfilesAPI.Endpoints.Receptionists.Update;

public class UpdateReceptionistEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("receptionists/{id:guid}", async (
            Guid id,
            [FromBody] UpdateReceptionistCommand request,
            ICommandHandler<UpdateReceptionistCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Accounts.Manage);
    }
}

public record UpdateReceptionistCommand(
    Guid Id,
    string OfficeId) : ICommand;

public class UpdateReceptionistCommandHandler(ProfilesDbContext context) : ICommandHandler<UpdateReceptionistCommand>
{
    public async Task<Result> Handle(UpdateReceptionistCommand command, CancellationToken ct)
    {
        var receptionist = await context.Receptionists
            .FirstOrDefaultAsync(x => x.AccountId == command.Id, cancellationToken: ct);

        if (receptionist == null) return ReceptionistErrors.NotFound();

        receptionist.OfficeId = command.OfficeId;
        
        await context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}

public class UpdateReceptionistCommandValidator : AbstractValidator<UpdateReceptionistCommand>
{
    public UpdateReceptionistCommandValidator()
    {
        // RuleFor(x => x.Property).NotEmpty();
    }
}
