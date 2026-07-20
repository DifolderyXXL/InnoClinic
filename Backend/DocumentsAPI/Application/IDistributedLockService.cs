namespace DocumentsAPI.Application;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> TryAcquireLockAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry, CancellationToken ct);
}