using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServicesAPI.Data;
using ServicesAPI.Models;
using SQLitePCL;

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
    Task<bool> TrySchedule(ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date, CancellationToken ct);
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


public interface IReservedTimeWindowStore
{
    public Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date, CancellationToken ct);
    public Task<bool> TryAdd(ReservedTimeWindow reservation, CancellationToken ct);
}

public class ReservedTimeWindowStore(ServicesDbContext context, ILogger<ReservedTimeWindowStore>? logger) : IReservedTimeWindowStore
{
    public async Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date, CancellationToken ct)
        => await context.ReservedTimeWindows
            .AsNoTracking()
            .Where(x => x.Date == date)
            .ToListAsync(cancellationToken: ct);
    

    public async Task<bool> TryAdd(ReservedTimeWindow reservation, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var hasOverlap = await context.ReservedTimeWindows.AnyAsync(x =>
                x.Date == reservation.Date
                && x.StartSlotIndex < (reservation.StartSlotIndex + reservation.SlotCount)
                && (x.StartSlotIndex + x.SlotCount) > reservation.StartSlotIndex, ct);

            if (hasOverlap)
            {
                await transaction.RollbackAsync(ct);
                return false;
            }

            await context.ReservedTimeWindows.AddAsync(reservation, ct);
            await context.SaveChangesAsync(ct);
            
            await transaction.CommitAsync(ct);
            return true;
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync(ct);
            
            logger?.LogError(ex, "Error on reserving time window {@Reservation}", reservation);
            return false;
        }
    }
}