using DocumentsAPI.Application;
using RedLockNet;

namespace DocumentsAPI.Infrastructure.Locking;

public class DistributedLockService(IDistributedLockFactory lockFactory) : IDistributedLockService
{
    public async Task<IAsyncDisposable?> TryAcquireLockAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry, CancellationToken ct)
    {
        var redLock = await lockFactory.CreateLockAsync(resource, expiry, wait, retry, ct);
        
        if (!redLock.IsAcquired)
        {
            await redLock.DisposeAsync();
            return null;
        }

        return redLock;
    }
}