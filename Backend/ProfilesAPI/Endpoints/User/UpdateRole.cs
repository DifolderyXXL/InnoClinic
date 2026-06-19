using System;
using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc;

namespace ProfilesAPI.Endpoints.User;

public class UpdateRole : IEndpoint
{
    class Request
    {
        public string UserId;
        public string Role;
    }
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/api/role", async ([FromBody] Request request, IHttpClientFactory factory, CancellationToken ct) =>
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

            return response.IsSuccessStatusCode ? Results.Created() : Results.BadRequest();
        })
        .RequireAuthorization(RolePolicy.Receptionist)
        .WithDescription("Updates role for user."); ;
    }

    private async Task<HttpResponseMessage> SendUpdateRole(HttpClient context, string UserId, params string[] Roles)
    {
        var response = await context.PutAsJsonAsync("role", new { UserId = UserId, Roles = Roles });
        return response;
    }
}
