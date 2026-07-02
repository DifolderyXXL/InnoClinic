using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public interface IReservedTimeWindowStore
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date, CancellationToken ct);
    public Task<bool> TryAdd(ReservedTimeWindow reservation, CancellationToken ct);
}