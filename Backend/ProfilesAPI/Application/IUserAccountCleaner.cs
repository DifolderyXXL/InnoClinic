using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Application;

public interface IUserAccountCleaner
{
    public Task<Result> DeleteUserProfilesAndAccount(Guid userId, CancellationToken ct);
}