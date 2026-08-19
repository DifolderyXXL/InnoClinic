using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;

namespace ProfilesAPI.Infrastructure;

public class UserAccountCleaner(ProfilesDbContext context) : IUserAccountCleaner
{
    public async Task<Result> DeleteUserProfilesAndAccount(Guid userId, CancellationToken ct)
    {
        try
        {
            await context.Accounts
                .Where(x => x.Id == userId)
                .ExecuteDeleteAsync(ct);
        }
        catch (Exception e)
        {
            return Error.Failure(
                "Profiles.AccountDeleteFailed",
                $"Failed to delete user account and associated profiles for UserId '{userId}'. Details: {e.Message}"
            );
        }

        return Result.Success();
    }
}