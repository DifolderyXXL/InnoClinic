using System.Data;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Application.Scheduling;

public class ReservedTimeWindowStore(ServicesDbContext context, ILogger<ReservedTimeWindowStore>? logger) : IReservedTimeWindowStore
{
    public async Task<List<ReservedTimeWindow>> GetReservedWindows(Guid doctorId, DateOnly date, CancellationToken ct)
        => await context.ReservedTimeWindows
            .AsNoTracking()
            .Where(x => x.Date == date && x.DoctorId == doctorId)
            .ToListAsync(cancellationToken: ct);
    

    public async Task Add(ReservedTimeWindow reservation, CancellationToken ct)
    {
        await context.ReservedTimeWindows.AddAsync(reservation, ct);
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

    public async Task<bool> TryRemove(long reservationId, bool force, CancellationToken ct)
    {
        try
        {
            var query = context.ReservedTimeWindows
                .Where(r => r.Id == reservationId);

            if (!force)
            {
                query = query.Where(r => !r.IsConfirmed);
            }
            
            int rowsAffected = await query
                .ExecuteDeleteAsync(ct);
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error on removing reservation {@ReservationId}", reservationId);
            return false;
        }
    }
}