namespace ServicesAPI.Models;

public class ReservedTimeWindow
{
    public long Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }
    
    public bool IsConfirmed { get; set; }
    public int EndSlotIndex => StartSlotIndex + SlotCount;
}