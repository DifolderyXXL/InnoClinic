using System.Text.Json.Serialization;
using Contracts.DocumentsContracts;
using FluentValidation;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Accounts.Update;

public class UpdateAccountEndpoint : IAccountEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/accounts/me", async (
            UpdateAccountCommand command,
            ICommandHandler<UpdateAccountCommand> handler,
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            var result = await handler.Handle(command with {Id = guid}, ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization();
        
        builder.MapPut("/accounts/{userId:guid}", async (
            Guid userId,
            UpdateAccountCommand command,
            ICommandHandler<UpdateAccountCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command with {Id = userId}, ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Accounts.Manage);
    }
}

public record UpdateAccountCommand([property: JsonIgnore]Guid Id, 
    string? FirstName, string? LastName, string? MiddleName, string? PhoneNumber, Guid? PhotoId) : ICommand;

public class UpdateAccountCommandHandle(
    ProfilesDbContext context,
    IPublishEndpoint publishEndpoint) : ICommandHandler<UpdateAccountCommand>
{
    public async Task<Result> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken: ct);
        if (account == null) return AccountErrors.NotFound();

        if (command.FirstName != null) account.FirstName = command.FirstName;
        if (command.LastName != null) account.LastName = command.LastName;
        if (command.MiddleName != null) account.MiddleName = command.MiddleName;
        if (command.PhoneNumber != null) account.PhoneNumber = command.PhoneNumber;
        if (command.PhotoId != null)
        {
            var oldId = account.PhotoId;
            account.PhotoId = command.PhotoId;
            
            var isDoctor = await context.Doctors.AnyAsync(x => x.AccountId == account.Id, ct);

            await publishEndpoint.Publish(new ConfirmProfilePhoto(account.Id, command.PhotoId.Value, oldId, isDoctor), ct);
        }
        
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
