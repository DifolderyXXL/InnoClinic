using Microsoft.Extensions.Options;
using SQLitePCL;

namespace ServicesAPI.Application.Scheduling;

public interface IScheduleService
{
    Task<bool> TrySchedule(ScheduleTimeWindow scheduleTimeWindow, CancellationToken ct);
    Task<IEnumerable<ScheduleTimeWindow>> GetAvailablePositionsOnDay(DateOnly date, CancellationToken ct);
}