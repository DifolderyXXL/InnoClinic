using Deunde.IdentityServer.Services;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deunde.IdentityServer.Endpoints.User;

public class GetUserByEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/by-email/{email}", async (
            string email,
            ICommandHandler<GetUserByEmailCommand, GetUserByEmailResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(email), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}

public record GetUserByEmailCommand(string Email) : ICommand<GetUserByEmailResponse>;

public record GetUserByEmailResponse(Guid UserId);

public class GetUserByEmailCommandHandler(
    UserManager<IdentityUser> userManager) : ICommandHandler<GetUserByEmailCommand, GetUserByEmailResponse>
{
    public async Task<Result<GetUserByEmailResponse>> Handle(GetUserByEmailCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user == null)
        {
            return new Error("User not found.", ErrorType.NotFound);
        }

        return new GetUserByEmailResponse(new Guid(user.Id));
    }
}

public class GetUserByEmailCommandValidator : AbstractValidator<GetUserByEmailCommand>
{
    public GetUserByEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
