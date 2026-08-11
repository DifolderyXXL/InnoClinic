using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public interface IReservedTimeWindowStore
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(Guid doctorId, Guid patientId, DateOnly date, CancellationToken ct);
    public Task Add(ReservedTimeWindow reservation, CancellationToken ct);
    public Task<bool> TryConfirm(long reservationId, CancellationToken ct);
    public Task<bool> TryRemove(long reservationId, bool force, CancellationToken ct);
}