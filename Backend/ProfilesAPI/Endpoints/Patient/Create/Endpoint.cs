using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Patient.Create;

public record PatientRequest(DateOnly DateOfBirth);
public class CreatePatientEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/patients/me", async (
            [FromBody] PatientRequest request,
            UserClaimInfo user,
            ICommandHandler<CreatePatientCommand> handler,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            var result = await handler.Handle(new(guid, request.DateOfBirth), ct);
            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization(RolePolicy.Client);
    }
}

public record CreatePatientCommand(Guid Id, DateOnly DateOfBirth) : ICommand;

public class CreatePatientCommandHandler(ProfilesDbContext context) : ICommandHandler<CreatePatientCommand>
{
    public async Task<Result> Handle(CreatePatientCommand command, CancellationToken ct)
    {  
        var account = await context.Accounts
            .Include(account => account.Patient)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        
        if (account == null) return AccountErrors.NotFound();

        if (account.Patient != null) return PatientErrors.AlreadyExists();

        await context.Patients.AddAsync(new() { Account = account, DateOfBirth = command.DateOfBirth }, ct);
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        // RuleFor(x => x.Property).NotEmpty();
    }
}
public static class PatientErrors
{
    public static Error AlreadyExists() => Error.Create(ErrorType.Conflict);
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}
