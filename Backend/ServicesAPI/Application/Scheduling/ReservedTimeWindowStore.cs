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

    public async Task<bool> TryConfirm(long reservationId, CancellationToken ct)
    {
        try
        {
            var reservation = await context.ReservedTimeWindows.FindAsync([reservationId], ct);

            if (reservation == null) return false;

            reservation.IsConfirmed = true;
            await context.SaveChangesAsync(ct);
            
            return true;
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
            var reservation = await context.ReservedTimeWindows.FindAsync([reservationId], ct);

            if (reservation == null) return false;
            
            context.ReservedTimeWindows.Remove(reservation);
            
            await context.SaveChangesAsync(ct);
            
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error on confirming reservation {@ReservationId}", reservationId);
            return false;
        }
    }
}