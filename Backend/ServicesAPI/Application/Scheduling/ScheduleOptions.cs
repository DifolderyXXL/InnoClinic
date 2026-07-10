using System.ComponentModel.DataAnnotations;

namespace ServicesAPI.Application.Scheduling;

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

    public TimeSpan GetSlotTime(int slot)
    {
        return WorkScheduleBeginTime + slot * TimeSlotLength;
    }
}