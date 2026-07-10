namespace Contracts.ProfilesContracts;

public record DoctorEventObject
{    
    public long Id { get; set; }
    public Guid AccountId { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public long OfficeId { get; set; }
    public long CareerStartYear { get; set; }
}

public record DoctorCreatedEvent : DoctorEventObject;
public record DoctorUpdatedEvent : DoctorEventObject;
public record DoctorDeletedEvent : DoctorEventObject;