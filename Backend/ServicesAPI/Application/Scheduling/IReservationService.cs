using Microsoft.Extensions.Options;
using SQLitePCL;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleResult(bool IsSuccess, long? ReservationId);
public interface IReservationService
{
    Task<ScheduleResult> TryReserve(ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<bool> TryConfirmReservation(long reservationId, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date, CancellationToken ct);
    Task CancelReservation(long reservationId, CancellationToken ct);
}