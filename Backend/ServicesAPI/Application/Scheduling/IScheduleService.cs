using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ServicesAPI.Application.Scheduling;

public record ScheduleTimeWindow(DateOnly Date, int TimeSlotStart, int TimeSlotSize)
{
    public static ScheduleTimeWindow TimeWindowFromBegin(DateOnly date, int nextTimeSlot) 
        => new (date, 0, nextTimeSlot);
    public static ScheduleTimeWindow TimeWindowToEnd(DateOnly date, int lastReservationEnd, int totalSlotsAmount) 
        => new(date, lastReservationEnd, totalSlotsAmount - lastReservationEnd);
}

public interface IScheduleService
{
    Task<bool> TrySchedule(ScheduleTimeWindow scheduleTimeWindow);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date);
}

public interface IScheduleSlotsProvider
{
    public int GetSlotsAmount();
}
public class ScheduleOptions : IScheduleSlotsProvider
{
    public const string SectionName = "Schedule";
    
    [Required]
    public TimeSpan WorkScheduleBeginTime { get; init; }
    [Required]
    public TimeSpan WorkScheduleEndTime { get; init; }
    [Required]
    public TimeSpan TimeSlotLength { get; init; }

    public int GetSlotsAmount()
    {
        var timeSlotDecimalAmount = (WorkScheduleEndTime - WorkScheduleBeginTime) / TimeSlotLength;
        return (int)Math.Floor(timeSlotDecimalAmount);
    }
}

public class ScheduleService(IReservedTimeWindowRepository repository, IScheduleSlotsProvider provider) : IScheduleService
{
    public Task<bool> TrySchedule(ScheduleTimeWindow scheduleTimeWindow)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date)
    {
        var reserved = (await repository.GetReservedWindows(date)).ToList();
        
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

        var allAvailableGaps = new List<ScheduleTimeWindow> { startGap };
        allAvailableGaps.AddRange(middleGaps);
        allAvailableGaps.Add(endGap);

        return allAvailableGaps.Where(w => w.TimeSlotSize > 0).ToList();
    }

    private ScheduleTimeWindow GetTimeSlotBetween(ReservedTimeWindow left, ReservedTimeWindow right)
    {
        var prevEnd = left.EndSlotIndex;
        var space = new ScheduleTimeWindow(left.Date, prevEnd, right.StartSlotIndex - prevEnd);
        return space;
    }
}


public class ReservedTimeWindow
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }
    
    public int EndSlotIndex => StartSlotIndex + SlotCount;
}
public interface IReservedTimeWindowRepository
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date);
    public Task Add(ReservedTimeWindow reservation, CancellationToken ct);
}

public class ReservedTimeWindowMemoryRepository : IReservedTimeWindowRepository
{
    private List<ReservedTimeWindow> _items = [];
    public Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date)
    {
        return Task.FromResult(_items);
    }

    public Task Add(ReservedTimeWindow reservation, CancellationToken ct)
    {
        _items.Add(reservation);
        return Task.CompletedTask;
    }
}