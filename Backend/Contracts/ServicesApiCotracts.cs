namespace Contracts;

public record SpecializationObject
{
    public long Id { get; set; }
    public string SpecializationName { get; set; }
    public bool IsActive { get; set; }
}

public record SpecializationUpdatedEvent : SpecializationObject;
public record SpecializationCreatedEvent : SpecializationObject;