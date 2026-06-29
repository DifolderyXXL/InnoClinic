namespace ProfilesAPI.Models;

public class Doctor
{
    public long Id { get; set; }
    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; }

    public DateOnly DateOfBirth { get; set; }
    public virtual Specialization Specialization { get; set; }
    public long OfficeId { get; set; }
    public long CareerStartYear { get; set; }

    public Status Status { get; set; }
}
