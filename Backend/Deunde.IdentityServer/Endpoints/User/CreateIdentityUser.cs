using System.Text;
using Deunde.IdentityServer.Services;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Deunde.IdentityServer.Endpoints.User;

public class CreateIdentityUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/users", async (
            [FromBody] CreateIdentityUserCommand request, 
            ICommandHandler<CreateIdentityUserCommand, CreateIdentityUserResponse> handler, 
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}

public record CreateIdentityUserCommand(
    string Email,
    List<string> Roles,
    string? ReturnUrl) : ICommand<CreateIdentityUserResponse>;

public record CreateIdentityUserResponse(string UserId, string SetPasswordLink);

public class CreateIdentityUserCommandHandler(
    IUserCreateManager createManager, 
    UserManager<IdentityUser> userManager,
    IHttpContextAccessor contextAccessor,
    LinkGenerator linkGenerator) : ICommandHandler<CreateIdentityUserCommand, CreateIdentityUserResponse>
{
    public async Task<Result<CreateIdentityUserResponse>> Handle(CreateIdentityUserCommand command, CancellationToken ct)
    {
        var context = contextAccessor.HttpContext;
        if (context == null)
        {
            return new Error("HttpContext is null.", ErrorType.Internal);
        }
        
        try
        {
            var user = await createManager.CreateExternal(command.Email, command.Roles.ToArray(), false);

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
            
            
            
            var setPasswordLink = linkGenerator.GetUriByPage(
                httpContext: context,
                page: "/Account/SetPassword/Index",
                handler: null,
                values: new { userId = user.Id, token = encodedToken, returnUrl = command.ReturnUrl }
            );
            
            if(setPasswordLink == null)
                return new Error("Set password link has not generated properly", ErrorType.Internal);
            
            return new CreateIdentityUserResponse(user.Id, setPasswordLink);
        }
        catch (InvalidOperationException e)
        {
            return new Error(e.Message, ErrorType.Conflict);
        }
    }
}

public class CreateIdentityUserCommandValidator : AbstractValidator<CreateIdentityUserCommand>
{
    public CreateIdentityUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Roles)
            .NotNull()
            .Must(roles => roles is { Count: > 0 });

        RuleForEach(x => x.Roles)
            .NotEmpty().WithMessage("Role cant be empty");
    }
}
