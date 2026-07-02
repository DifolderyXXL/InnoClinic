namespace ServicesAPI.Models;

public class ReservedTimeWindow
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }
    
    public int EndSlotIndex => StartSlotIndex + SlotCount;
}