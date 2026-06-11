using AuthorizationAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationAPI.Endpoints.Auth;

public class Register : IEndpoint
{
    public class Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Response
    {

    }

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/core/auth/register", async ([FromBody] Request request,
                                                      [FromQuery] bool useCookies,
                                                      [FromQuery] bool rememberMe,
                                                      HttpContext httpContext,
                                                      SignInManager<IdentityUser> signInManager,
                                                      UserManager<IdentityUser> userManager,
                                                      ISessionAuthorizeManager sessionAuthorizeManager,
                                                      ILogger<Register> logger) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email,  };
            var result = await userManager.CreateAsync(user, request.Password);
            

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }


            bool isEmailVerificationRequired = userManager.Options.SignIn.RequireConfirmedEmail;
            if (isEmailVerificationRequired)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                string scheme = httpContext.Request.Scheme;
                string host = httpContext.Request.Host.Value;

                string baseHostLink = $"{scheme}://{host}";

                string approveLink = $"{baseHostLink}/core/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}&useCookies={useCookies}&rememberMe={rememberMe}";

                logger.LogInformation($"Email confirmation link for user {user.Email}: {approveLink}");
            }

            await sessionAuthorizeManager.AuthorizeSession(user, useCookies, rememberMe);

            return Results.Ok();
        });

        builder.MapPost("/core/auth/login", async ([FromBody] Request request,
                                                   [FromQuery] bool useCookies,
                                                   [FromQuery] bool rememberMe,
                                                   SignInManager<IdentityUser> signInManager,
                                                   UserManager<IdentityUser> userManager,
                                                   ISessionAuthorizeManager sessionAuthorizeManager) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email, };
            var result = await userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                return Results.Unauthorized();
            }

            if (!await sessionAuthorizeManager.AuthorizeSession(user, useCookies, rememberMe))
                return Results.Unauthorized();

            return Results.Ok();
        });

        builder.MapPost("/core/auth/logout", async (SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        }).RequireAuthorization();

        builder.MapGet("/core/auth/confirm-email", async (string userId,
                                                           string token,
                                                           [FromQuery] bool useCookies,
                                                           [FromQuery] bool rememberMe,
                                                           SignInManager<IdentityUser> signInManager,
                                                           UserManager<IdentityUser> userManager,
                                                           ISessionAuthorizeManager sessionAuthorizeManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.BadRequest();
            }
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return Results.BadRequest("Email confirmation failed.");
            }

            if (!await sessionAuthorizeManager.AuthorizeSession(user, useCookies, rememberMe))
                return Results.Unauthorized();

            return Results.Ok("Email confirmed successfully.");
        });
    }
}
