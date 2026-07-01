namespace Contracts;

public record SpecializationEventObject
{
    public long Id { get; init; }
    public string SpecializationName { get; init; }
    public bool IsActive { get; init; }
}
public record SpecializationUpdatedEvent : SpecializationEventObject;
public record SpecializationCreatedEvent : SpecializationEventObject;
public record SpecializationDeletedEvent : SpecializationEventObject;



public record ServiceEventObject
{
    public long Id { get; init; }

    public long CategoryId { get; init; }
    public string ServiceName { get; init; }
    public decimal Price { get; init; }

    public long SpecializationId { get; init; }
    public bool IsActive { get; init; }
}
public record ServiceUpdatedEvent : ServiceEventObject;
public record ServiceCreatedEvent : ServiceEventObject;
public record ServiceDeletedEvent : ServiceEventObject;

public record CategoryEventObject
{
    public long Id { get; init; }
    public string CategoryName { get; init; }
    public TimeSpan TimeSlotSize { get; init; }
}
public record CategoryUpdatedEvent : CategoryEventObject;
public record CategoryCreatedEvent : CategoryEventObject;
public record CategoryDeletedEvent : CategoryEventObject;
