using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Accounts.Update;

public class UpdateAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/accounts/me", async (
            UpdateAccountCommand command,
            ICommandHandler<UpdateAccountCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization();
    }
}

public record UpdateAccountCommand(Guid Id, string? FirstName, string? LastName, string? MiddleName, string? PhoneNumber) : ICommand;

public class UpdateAccountCommandHandle(ProfilesDbContext context) : ICommandHandler<UpdateAccountCommand>
{
    public async Task<Result> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken: ct);
        if (account == null) return AccountErrors.NotFound();

        if (command.FirstName != null) account.FirstName = command.FirstName;
        if (command.LastName != null) account.LastName = command.LastName;
        if (command.MiddleName != null) account.MiddleName = command.MiddleName;
        if (command.PhoneNumber != null) account.PhoneNumber = command.PhoneNumber;

        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.FirstName).MaximumLength(64);
        RuleFor(x => x.MiddleName).MaximumLength(64);
        RuleFor(x => x.LastName).MaximumLength(64);
        RuleFor(x => x.PhoneNumber).MaximumLength(64);
    }
}
