using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public static class ScheduleCalculator
{
    public static IEnumerable<ScheduleTimeWindow> CalculateAvailableGaps(
        DateOnly date, 
        List<ReservedTimeWindow> reserved, 
        int totalSlotsAmount)
    {
        if (reserved.Count == 0)
        {
            return [new ScheduleTimeWindow(date, 0, totalSlotsAmount)];
        }
        
        var reservedSorted = reserved.OrderBy(x => x.StartSlotIndex).ToList();

        var middleGaps = reservedSorted.Zip(reservedSorted.Skip(1), GetTimeSlotBetween);
        var firstReservation = reservedSorted.First();
        var lastReservation = reservedSorted.Last();

        var startGap = ScheduleTimeWindow.TimeWindowFromBegin(date, firstReservation.StartSlotIndex);
        var endGap = ScheduleTimeWindow.TimeWindowToEnd(date, lastReservation.EndSlotIndex, totalSlotsAmount);
        
        return new[] { startGap }.Concat(middleGaps).Concat([endGap])
            .Where(w => w.TimeSlotSize > 0);
    }
    
    public static ScheduleTimeWindow GetTimeSlotBetween(ReservedTimeWindow left, ReservedTimeWindow right)
    {
        var prevEnd = left.EndSlotIndex;
        var space = new ScheduleTimeWindow(left.Date, prevEnd, right.StartSlotIndex - prevEnd);
        return space;
    }
}