using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc;

namespace ProfilesAPI.Endpoints.User;

public class UpdateRole : IEndpoint
{
    public record Request(string UserId, string Role, string Action);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/accounts/role", async ([FromBody] Request request, IHttpClientFactory factory, CancellationToken ct) =>
            {
                var targetRole = request.Role switch
                {
                    "Doctor" => "doctor",
                    "Receptionist" => "receptionist",
                    _ => null
                };

                if (targetRole == null)
                {
                    return Results.BadRequest(new { error = "Invalid role" });
                }

                var client = factory.CreateClient("identityclient");
                HttpResponseMessage response;

                var payload = new { UserId = request.UserId, Role = targetRole };

                if (request.Action.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    response = await client.PostAsJsonAsync("role", payload, ct);
                }
                else if (request.Action.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "role")
                    {
                        Content = JsonContent.Create(payload)
                    };
                    response = await client.SendAsync(httpRequest, ct);
                }
                else
                {
                    return Results.BadRequest(new { error = "Invalid action" });
                }

                return response.IsSuccessStatusCode ? Results.Ok() : Results.BadRequest();
            })
            .RequireAuthorization(RolePolicy.Receptionist)
            .WithDescription("Updates role for user."); ;
    }
}
