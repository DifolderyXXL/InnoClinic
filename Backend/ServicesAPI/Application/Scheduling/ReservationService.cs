using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public class ReservationService(IReservedTimeWindowStore store, IScheduleSlotsProvider provider) : IReservationService
{
    public async Task<ReservedTimeWindow> TryReserve(Guid doctorId, Guid patientId, Guid appointmentId, ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct)
    {
        var reservation = new ReservedTimeWindow
        {
            DoctorId = doctorId,
            AppointmentId = appointmentId,
            StartSlotIndex = scheduleTimeWindow.TimeSlotStart, 
            SlotCount = scheduleTimeWindow.TimeSlotSize,
            Date = scheduleTimeWindow.Date,
            PatientId = patientId,
            IsConfirmed = false
        };
        
        await store.Add(reservation, ct);
        return reservation;
    }

    public async Task<bool> TryConfirmReservation(long reservationId, CancellationToken ct)
    {
        return await store.TryConfirm(reservationId, ct);
    }

    public async Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(Guid doctorId, Guid patientId, DateOnly date, CancellationToken ct)
    {
        var reserved = (await store.GetReservedWindows(doctorId, patientId, date, ct)).ToList();
        
        var slotAmount = provider.GetSlotsAmount();
        
        return ScheduleCalculator.CalculateAvailableGaps(date, reserved, slotAmount);
    }

    public async Task CancelReservation(long reservationId, bool force, CancellationToken ct)
    {
        await store.TryRemove(reservationId, force, ct);
    }
}