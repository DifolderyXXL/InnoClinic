using Microsoft.Extensions.Options;
using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleResult(bool IsSuccess, long? ReservationId);
public interface IReservationService
{
    Task<ReservedTimeWindow> TryReserve(Guid doctorId, Guid patientId, Guid appointmentId, ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<bool> TryConfirmReservation(long reservationId, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(Guid doctorId, Guid patientId, DateOnly date, CancellationToken ct);
    Task CancelReservation(long reservationId, bool force, CancellationToken ct);
}