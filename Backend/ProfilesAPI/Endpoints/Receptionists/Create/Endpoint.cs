using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Receptionists.Create;

public class CreateReceptionistEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("receptionists/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] CreateReceptionistCommand command,
            ICommandHandler<CreateReceptionistCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command with { AccountId = id }, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).HasPermissions(Permissions.Accounts.Manage);
    }
}
public record CreateReceptionistCommand(
    Guid AccountId,
    string OfficeId) : ICommand;

public class CreateReceptionistCommandHandler(ProfilesDbContext context) : ICommandHandler<CreateReceptionistCommand>
{
    public async Task<Result> Handle(CreateReceptionistCommand command, CancellationToken ct)
    {
        var account = await context.Accounts
            .Include(account => account.Receptionist)
            .FirstOrDefaultAsync(x => x.Id == command.AccountId, ct);

        if (account == null) return AccountErrors.NotFound();

        if (account.Receptionist != null) return ReceptionistErrors.AlreadyExists();
        
        await context.Receptionists.AddAsync(new()
        {
            AccountId = account.Id,
            OfficeId = command.OfficeId,
        }, ct);

        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateReceptionistCommandValidator : AbstractValidator<CreateReceptionistCommand>
{
    public CreateReceptionistCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.OfficeId).NotEmpty();
    }
}

public class ReceptionistErrors : DomainErrors<ReceptionistErrors>;
