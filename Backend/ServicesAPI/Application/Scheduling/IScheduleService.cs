using Microsoft.Extensions.Options;
using SQLitePCL;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleResult(bool IsSuccess, long? ReservationId);
public interface IScheduleService
{
    Task<ScheduleResult> TrySchedule(ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<bool> TryConfirmSchedule(long reservationId, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date, CancellationToken ct);
    Task CancelSchedule(long reservationId, CancellationToken ct);
}