using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Patient;

public class Create : IEndpoint
{
    public record AccountRequest(string FirstName, string LastName, string? MiddleName, string? PhoneNumber);
    public record PatientRequest(DateOnly DateOfBirth);
    public record DoctorRequest(DateOnly DateOfBirth, long OfficeId);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/create/account", async (
            [FromBody] AccountRequest request,
            UserClaimInfo user,
            ProfilesDbContext context,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);

            var accountExists = await context.Accounts.AnyAsync(x => x.Id == guid);
            if (accountExists) return Results.BadRequest("Account already created");

            await context.Accounts.AddAsync(new()
            {
                Id = guid,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = user.Email,
                PhoneNumber = request.PhoneNumber,
                IsEmailVerified = user.EmailVerified,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, ct);
            await context.SaveChangesAsync(ct);

            return Results.Created();
        }).RequireAuthorization(RolePolicy.Client);

        builder.MapPost("/create/patient", async (
            PatientRequest request,
            UserClaimInfo user,
            ProfilesDbContext context,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);

            var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == guid);
            if (account == null) return Results.BadRequest("Account is not created");

            if (account.Patient != null) return Results.BadRequest("Patient already created");

            await context.Patients.AddAsync(new() { Account = account, DateOfBirth = request.DateOfBirth });
            await context.SaveChangesAsync(ct);

            return Results.Created();
        }).RequireAuthorization(RolePolicy.Client);
    }
}