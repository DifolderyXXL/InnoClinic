using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
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
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            var result = await handler.Handle(command with {Id = guid}, ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization();
    }
}

public record UpdateAccountCommand(Guid Id, string? FirstName, string? LastName, string? MiddleName, string? PhoneNumber, bool? PhotoChanged) : ICommand;

public class UpdateAccountCommandHandle(ProfilesDbContext context, IHttpClientFactory factory, ILogger<UpdateAccountCommandHandle> logger) : ICommandHandler<UpdateAccountCommand>
{
    public async Task<Result> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken: ct);
        if (account == null) return AccountErrors.NotFound();

        if (command.FirstName != null) account.FirstName = command.FirstName;
        if (command.LastName != null) account.LastName = command.LastName;
        if (command.MiddleName != null) account.MiddleName = command.MiddleName;
        if (command.PhoneNumber != null) account.PhoneNumber = command.PhoneNumber;

        var oldPhotoId = account.PhotoId;
        var newPhotoId = command.PhotoChanged.GetValueOrDefault() ? Guid.NewGuid() : account.PhotoId;
        account.PhotoId = newPhotoId;
        await context.SaveChangesAsync(ct);

        if (command.PhotoChanged.GetValueOrDefault() && oldPhotoId != newPhotoId)
        {
            var client = factory.CreateClient("documentsclient");
            try
            {
                var result = await client.PostAsync($"api/v1/Photos/users/{account.Id}/avatar/confirm?photoId={newPhotoId}&oldPhotoId={oldPhotoId}", null, ct);
                
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Photo is not confirmed");
            }
        }
        
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
