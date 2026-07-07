using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc;

namespace ProfilesAPI.Endpoints.User;

public class UpdateRole : IEndpoint
{
    public record Request(string UserId, string Role);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/role", async ([FromBody] Request request, IHttpClientFactory factory, CancellationToken ct) =>
        {
            var context = factory.CreateClient("identityclient");
            HttpResponseMessage? response = request.Role switch
            {
                "Patient" => await SendUpdateRole(context, request.UserId, "client"),
                "Doctor" => await SendUpdateRole(context, request.UserId, "client", "doctor"),
                "Receptionist" => await SendUpdateRole(context, request.UserId, "client", "doctor", "receptionist"),
                _ => null
            };
            if (response == null)
                return Results.BadRequest();

            return response.IsSuccessStatusCode ? Results.Ok() : Results.BadRequest();
        })
        .RequireAuthorization(RolePolicy.Receptionist)
        .WithDescription("Updates role for user."); ;
    }

    private async Task<HttpResponseMessage> SendUpdateRole(HttpClient context, string userId, params string[] roles)
    {
        var response = await context.PutAsJsonAsync("role", new { UserId = userId, Roles = roles });
        return response;
    }
}
