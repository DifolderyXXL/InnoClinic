using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public interface IReservedTimeWindowStore
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date, CancellationToken ct);
    public Task<bool> TryAdd(ReservedTimeWindow reservation, CancellationToken ct);
    public Task<bool> TryConfirm(long reservationId, CancellationToken ct);
    public Task<bool> TryRemove(long reservationId, CancellationToken ct);
}