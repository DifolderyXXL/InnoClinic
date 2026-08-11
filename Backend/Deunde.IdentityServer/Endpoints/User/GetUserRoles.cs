using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Endpoints.User;

public class GetUserRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/{userId:guid}/roles", async(
            Guid userId,
            IQueryHandler< GetUserRolesQuery, List<string>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(userId), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Accounts.Read)
        .RequireAuthorization(new AuthorizeAttribute 
        { 
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme 
        });
    }
}

public record GetUserRolesQuery(Guid UserId) : IQuery<List<string>>;

public class GetUserRolesQueryHandler( UserManager<IdentityUser> userManager) : IQueryHandler<GetUserRolesQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(GetUserRolesQuery query, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
        {
            return UserErrors.UserNotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        return roles.ToList();
    }
}