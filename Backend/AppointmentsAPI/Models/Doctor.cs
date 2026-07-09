namespace AppointmentsAPI.Models;

public class Doctor
{
    public long Id { get; set; }
    public Guid AccountId { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public long OfficeId { get; set; }
    public long CareerStartYear { get; set; }
    
    public ICollection<Appointment> Appointments { get; set; }
}