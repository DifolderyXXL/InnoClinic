using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public class ReservationService(IReservedTimeWindowStore store, IScheduleSlotsProvider provider) : IReservationService
{
    public async Task<ScheduleResult> TryReserve(Guid doctorId, Guid appointmentId, ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct)
    {
        var reservation = new ReservedTimeWindow
        {
            DoctorId = doctorId,
            AppointmentId = appointmentId,
            StartSlotIndex = scheduleTimeWindow.TimeSlotStart, 
            SlotCount = scheduleTimeWindow.TimeSlotSize,
            Date = scheduleTimeWindow.Date,
            IsConfirmed = false
        };
        
        var result = await store.TryAdd(reservation, ct);
        return new ScheduleResult(result, result ? reservation.Id : null);
    }

    public async Task<bool> TryConfirmReservation(long reservationId, CancellationToken ct)
    {
        return await store.TryConfirm(reservationId, ct);
    }

    public async Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(Guid doctorId, DateOnly date, CancellationToken ct)
    {
        var reserved = (await store.GetReservedWindows(doctorId, date, ct)).ToList();
        
        var slotAmount = provider.GetSlotsAmount();
        if (reserved.Count == 0)
        {
            return [new ScheduleTimeWindow(date, 0, slotAmount)];
        }
        
        var reservedSorted = reserved.OrderBy(x => x.StartSlotIndex).ToList();

        var middleGaps = reservedSorted.Zip(reservedSorted.Skip(1), GetTimeSlotBetween);
        var firstReservation = reservedSorted.First();
        var lastReservation = reservedSorted.Last();

        var startGap = ScheduleTimeWindow.TimeWindowFromBegin(date, firstReservation.StartSlotIndex);
        var endGap = ScheduleTimeWindow.TimeWindowToEnd(date, lastReservation.EndSlotIndex, slotAmount);
        
        List<ScheduleTimeWindow> allAvailableGaps = [startGap, ..middleGaps, endGap];

        return allAvailableGaps.Where(w => w.TimeSlotSize > 0).ToList();
    }

    public async Task CancelReservation(long reservationId, CancellationToken ct)
    {
        await store.TryRemove(reservationId, true, ct);
    }

    private static ScheduleTimeWindow GetTimeSlotBetween(ReservedTimeWindow left, ReservedTimeWindow right)
    {
        var prevEnd = left.EndSlotIndex;
        var space = new ScheduleTimeWindow(left.Date, prevEnd, right.StartSlotIndex - prevEnd);
        return space;
    }
}