using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public class ScheduleService(IReservedTimeWindowStore store, IScheduleSlotsProvider provider) : IScheduleService
{
    public async Task<bool> TrySchedule(ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct)
    {
        return await store.TryAdd(
            new ReservedTimeWindow
            {
                StartSlotIndex = scheduleTimeWindow.TimeSlotStart, SlotCount = scheduleTimeWindow.TimeSlotSize,
                Date = scheduleTimeWindow.Date
            }, ct);
    }

    public async Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date, CancellationToken ct)
    {
        var reserved = (await store.GetReservedWindows(date, ct)).ToList();
        
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

    private static ScheduleTimeWindow GetTimeSlotBetween(ReservedTimeWindow left, ReservedTimeWindow right)
    {
        var prevEnd = left.EndSlotIndex;
        var space = new ScheduleTimeWindow(left.Date, prevEnd, right.StartSlotIndex - prevEnd);
        return space;
    }
}