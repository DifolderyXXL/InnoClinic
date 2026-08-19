using Microsoft.Extensions.Caching.Distributed;

namespace BFF.FrontendProxy.Services;

public interface IRevokedUserRepository
{
    public Task<bool> IsUserRevoked(string userId, CancellationToken ct);
    public Task RevokeUser(string userId, CancellationToken ct);
}

public class RedisRevokedUserRepository(IDistributedCache cache) : IRevokedUserRepository
{
    public async Task<bool> IsUserRevoked(string userId, CancellationToken ct)
    {
        var isRevoked = await cache.GetStringAsync($"revoked_user:{userId}", token: ct);
        return isRevoked != null;
    }

    public async Task RevokeUser(string userId, CancellationToken ct)
    {
        await cache.SetStringAsync($"revoked_user:{userId}", "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            },
            token: ct);
    }
}