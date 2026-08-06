using Contracts.Notifications;
using FluentValidation;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Accounts.Create;

public record AdminCreateAccountCommand(
    string Email, 
    string FirstName, 
    string LastName, 
    string? MiddleName, 
    string? PhoneNumber,
    List<string> Roles
) : ICommand;

public class AdminCreateAccountCommandHandler(
    ProfilesDbContext context,
    IIdentityServiceClient identityServiceClient,
    IPublishEndpoint publishEndpoint) : ICommandHandler<AdminCreateAccountCommand>
{
    public async Task<Result> Handle(AdminCreateAccountCommand command, CancellationToken ct)
    {
        var emailExists = await context.Accounts.AnyAsync(x => x.Email == command.Email, cancellationToken: ct);
        if (emailExists) return AccountErrors.AlreadyExists();

        var identityResult = await identityServiceClient.CreateIdentityUserAsync(
            command.Email,
            command.Roles,
            ct);
        
        if (identityResult.IsError)
        {
            return identityResult.Error!;
        }
        
        var (userId, setPasswordLink) = identityResult.Value!;
        var account = new Account
        {
            Id = userId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            MiddleName = command.MiddleName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        await context.Accounts.AddAsync(account, ct);
        
        var integrationEvent = new UserRegisteredIntegrationEvent(
            account.Id,
            account.Email,
            setPasswordLink
        );

        await publishEndpoint.Publish(integrationEvent, ct);

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AdminCreateAccountCommandValidator : AbstractValidator<AdminCreateAccountCommand>
{
    public AdminCreateAccountCommandValidator()
    {
        RuleFor(x => x.Email).ValidPersonName();
        RuleFor(x => x.FirstName).ValidPersonName();
        RuleFor(x => x.LastName).ValidPersonName();
    }
}