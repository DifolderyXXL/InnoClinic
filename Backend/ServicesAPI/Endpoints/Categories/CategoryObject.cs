namespace ServicesAPI.Endpoints.Categories;

public abstract record CategoryObject
{
    public required string CategoryName { get; init; }
    public required uint TimeSlotSize { get; init; }
}