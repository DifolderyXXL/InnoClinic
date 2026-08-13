using Microsoft.Extensions.Options;
using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleResult(bool IsSuccess, long? ReservationId);
public interface IReservationService
{
    Task<ReservedTimeWindow> TryReserve(Guid doctorId, Guid patientId, Guid appointmentId, ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(Guid doctorId, Guid patientId, DateOnly date, CancellationToken ct);
}

public interface IReservationLifecycleManager
{
    Task<bool> ConfirmAsync(long reservationId, CancellationToken ct);
    Task<bool> CancelAsync(long reservationId, bool force, CancellationToken ct);
    Task<bool> RescheduleAsync(long reservationId, DateOnly date, int startSlotIndex, CancellationToken ct);
}