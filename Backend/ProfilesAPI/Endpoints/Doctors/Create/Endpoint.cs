using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;
using ProfilesAPI.Endpoints.Doctors.GetDoctorById;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.Create;

public class CreateDoctorEndpoint : IDoctorEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("doctors/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] CreateDoctorCommand command,
            ICommandHandler<CreateDoctorCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command with {AccountId = id}, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}

public record CreateDoctorCommand(
    Guid AccountId,
    DateOnly DateOfBirth,
    long CareerStartYear,
    long SpecializationId,
    Status Status,
    string OfficeId) : ICommand;

public class CreateDoctorCommandHandler(ProfilesDbContext context) : ICommandHandler<CreateDoctorCommand>
{
    public async Task<Result> Handle(CreateDoctorCommand command, CancellationToken ct)
    {
        var account = await context.Accounts
            .Include(account => account.Doctor)
            .FirstOrDefaultAsync(x => x.Id == command.AccountId, ct);
        
        if (account == null) return AccountErrors.NotFound();

        if (account.Doctor != null) return DoctorErrors.AlreadyExists();
        
        var specialization = await context.Specializations.FindAsync([command.SpecializationId], ct);
        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }
        
        await context.Doctors.AddAsync(new()
        {
            AccountId = account.Id,
            CareerStartYear = command.CareerStartYear,
            DateOfBirth = command.DateOfBirth,
            OfficeId = command.OfficeId,
            Status = command.Status,
            Specialization = specialization
        }, ct);

        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
    }
}
public static class DoctorErrors
{
    public static Error AlreadyExists() => Error.Create(ErrorType.Conflict);
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}
