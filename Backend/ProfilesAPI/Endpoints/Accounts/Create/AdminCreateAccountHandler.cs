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
    IPublishEndpoint publishEndpoint,
    ILogger<AdminCreateAccountCommandHandler> logger) : ICommandHandler<AdminCreateAccountCommand>
{
    public async Task<Result> Handle(AdminCreateAccountCommand command, CancellationToken ct)
    {
        var emailExists = await context.Accounts.AnyAsync(x => x.Email == command.Email, cancellationToken: ct);
        if (emailExists) return AccountErrors.AlreadyExists();
        
        var userResult = await identityServiceClient.GetIdentityUserAsync(
            command.Email,
            ct);
        
        var account = new Account
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            MiddleName = command.MiddleName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        if (userResult.IsSuccess)
        {
            account.Id = userResult.Value!.UserId;
        
            await context.Accounts.AddAsync(account, ct);
            
            await context.SaveChangesAsync(ct);
            
            logger.LogInformation(
                "Successfully bound existing Identity user {UserId} with email {Email} to a new Profile account", 
                account.Id, 
                command.Email);
            
            return Result.Success();
        }
        
        if (userResult.Error?.ErrorType != ErrorType.NotFound)
        {
            logger.LogWarning(
                "Failed to query IdentityService for email {Email}. ErrorType: {ErrorType}", 
                command.Email, 
                userResult.Error?.ErrorType);
            
            return userResult.Error!;
        }

        var identityResult = await identityServiceClient.CreateIdentityUserAsync(
            command.Email,
            command.Roles,
            ct);
        
        if (identityResult.IsError)
        {
            logger.LogWarning(
                "IdentityService failed to create user for email {Email}. ErrorType: {ErrorType}", 
                command.Email, 
                identityResult.Error?.ErrorType);
            
            return identityResult.Error!;
        }
        
        var (userId, setPasswordLink) = identityResult.Value!;
        account.Id = Guid.Parse(userId);
        
        await context.Accounts.AddAsync(account, ct);
        
        var integrationEvent = new UserRegisteredIntegrationEvent(
            account.Id,
            account.Email,
            setPasswordLink
        );

        await publishEndpoint.Publish(integrationEvent, ct);

        await context.SaveChangesAsync(ct);
        
        logger.LogInformation(
            "Successfully created new Identity user and Profile account {AccountId} for email {Email}.", 
            account.Id, 
            command.Email);

        return Result.Success();
    }
}

public class AdminCreateAccountCommandValidator : AbstractValidator<AdminCreateAccountCommand>
{
    public AdminCreateAccountCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.FirstName).ValidPersonName();
        RuleFor(x => x.LastName).ValidPersonName();
    }
}