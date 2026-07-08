using System.Data;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public class ReservedTimeWindowStore(ServicesDbContext context, ILogger<ReservedTimeWindowStore>? logger) : IReservedTimeWindowStore
{
    public async Task<List<ReservedTimeWindow>> GetReservedWindows(DateOnly date, CancellationToken ct)
        => await context.ReservedTimeWindows
            .AsNoTracking()
            .Where(x => x.Date == date)
            .ToListAsync(cancellationToken: ct);
    

    public async Task<bool> TryAdd(ReservedTimeWindow reservation, CancellationToken ct)
    {
        var hasOverlap = await context.ReservedTimeWindows.AnyAsync(x =>
            x.Date == reservation.Date
            && x.StartSlotIndex < (reservation.StartSlotIndex + reservation.SlotCount)
            && (x.StartSlotIndex + x.SlotCount) > reservation.StartSlotIndex, ct);

        if (hasOverlap)
        {
            return false;
        }

        await context.ReservedTimeWindows.AddAsync(reservation, ct);
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> TryConfirm(long reservationId, CancellationToken ct)
    {
        try
        {
            int rowsAffected = await context.ReservedTimeWindows.Where(x => x.Id == reservationId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsConfirmed, true), ct);
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error on confirming reservation {@ReservationId}", reservationId);
            return false;
        }
    }

    public async Task<bool> TryRemove(long reservationId, CancellationToken ct)
    {
        try
        {
            int rowsAffected = await context.ReservedTimeWindows
                .Where(r => r.Id == reservationId && !r.IsConfirmed)
                .ExecuteDeleteAsync(ct);
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error on confirming reservation {@ReservationId}", reservationId);
            return false;
        }
    }
}