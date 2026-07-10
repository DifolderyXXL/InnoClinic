using Microsoft.Extensions.Options;
using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleResult(bool IsSuccess, long? ReservationId);
public interface IReservationService
{
    Task<ReservedTimeWindow> TryReserve(Guid doctorId, Guid appointmentId, ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<bool> TryConfirmReservation(long reservationId, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(Guid doctorId, DateOnly date, CancellationToken ct);
    Task CancelReservation(long reservationId, CancellationToken ct);
}