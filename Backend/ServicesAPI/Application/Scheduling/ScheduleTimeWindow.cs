namespace ServicesAPI.Application.Scheduling;

public record ScheduleTimeWindow(DateOnly Date, int TimeSlotStart, int TimeSlotSize)
{
    public static ScheduleTimeWindow TimeWindowFromBegin(DateOnly date, int nextTimeSlot) 
        => new (date, 0, nextTimeSlot);
    public static ScheduleTimeWindow TimeWindowToEnd(DateOnly date, int lastReservationEnd, int totalSlotsAmount) 
        => new(date, lastReservationEnd, totalSlotsAmount - lastReservationEnd);
}