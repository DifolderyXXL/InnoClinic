using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Patients.Create;

public record PatientRequest(DateOnly DateOfBirth);
public class CreatePatientEndpoint : IPatientEndpoint
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
        }).HasPermissions(Permissions.Patients.ManageOwn);
        
        builder.MapPut("/patients/me", async (
            [FromBody] UpdatePatientRequest request,
            UserClaimInfo user,
            ICommandHandler<UpdatePatientCommand> handler,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            var result = await handler.Handle(new UpdatePatientCommand(guid, request.DateOfBirth), ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Patients.ManageOwn);
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
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .ValidDateOfBirth();
    }
}

public record UpdatePatientRequest(DateOnly DateOfBirth);

public record UpdatePatientCommand(Guid Id, DateOnly DateOfBirth) : ICommand;
public class UpdatePatientCommandHandler(ProfilesDbContext context) : ICommandHandler<UpdatePatientCommand>
{
    public async Task<Result> Handle(UpdatePatientCommand command, CancellationToken ct)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(x => x.AccountId == command.Id, ct);

        if (patient == null) 
            return PatientErrors.NotFound();

        patient.DateOfBirth = command.DateOfBirth;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .ValidDateOfBirth();
    }
}


public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, DateOnly> ValidDateOfBirth<T>(
        this IRuleBuilder<T, DateOnly> ruleBuilder)
    {
        return ruleBuilder
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");
    }
}