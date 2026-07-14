using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Accounts.Create;

public class CreateAccountEndpoint : IEndpoint
{
    public record AccountRequest(string FirstName, string LastName, string? MiddleName, string? PhoneNumber);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/accounts/me", async (
            [FromBody] AccountRequest request,
            ICommandHandler<CreateAccountCommand> handler, 
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            var result = await handler.Handle(
                new(
                    guid, 
                    request.FirstName,
                    request.LastName,
                    request.MiddleName,
                    request.PhoneNumber,
                    user.Email,
                    user.EmailVerified
                    ), ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization();
    }
}

public record CreateAccountCommand(Guid Id, string FirstName, string LastName, string? MiddleName, string? PhoneNumber, string Email, bool IsEmailVerified) : ICommand;

public class CreateAccountCommandHandler(ProfilesDbContext context) : ICommandHandler<CreateAccountCommand>
{
    public async Task<Result> Handle(CreateAccountCommand command, CancellationToken ct)
    {
        var accountExists = await context.Accounts.AnyAsync(x => x.Id == command.Id, cancellationToken: ct);
        if (accountExists) return AccountErrors.AlreadyExists();

        await context.Accounts.AddAsync(new()
        {
            Id = command.Id,
            FirstName = command.FirstName,
            LastName = command.LastName,
            MiddleName = command.MiddleName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            IsEmailVerified = command.IsEmailVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        }, ct);
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}