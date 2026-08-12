using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public interface IReservedTimeWindowStore
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(Guid doctorId, Guid patientId, DateOnly date, CancellationToken ct);
    public Task Add(ReservedTimeWindow reservation, CancellationToken ct);
}