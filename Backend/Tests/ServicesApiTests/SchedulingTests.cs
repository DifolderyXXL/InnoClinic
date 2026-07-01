using Microsoft.Extensions.Options;
using Moq;
using ServicesAPI.Application.Scheduling;

namespace ServicesApiTests;

public class SchedulingTests
{
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_OnePosition()
    {
        // Arrange
        DateOnly day = DateOnly.MinValue;
        
        int slotsAmount = 100;
        int targetPoint = 50;
        int targetPointSize = 10;
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        var repository = new ReservedTimeWindowMemoryRepository();

        await repository.Add(new() { Date = day, StartSlotIndex = targetPoint, SlotCount = targetPointSize }, default);
        
        var service = new ScheduleService(repository, option.Object);

        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(day);


        // Assert
        var assuming = new List<ScheduleTimeWindow>
        {
            new(day, 0, 50),
            new(day, 60, 40)
        };
        Assert.Equal(assuming, positions);
    }
    
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_TwoPositionInRow()
    {
        // Arrange
        DateOnly day = DateOnly.MinValue;
        
        int slotsAmount = 100;
        int targetPoint = 50;
        int targetPoint2 = 60;
        int targetPointSize = 10;
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        var repository = new ReservedTimeWindowMemoryRepository();

        await repository.Add(new() { Date = day, StartSlotIndex = targetPoint, SlotCount = targetPointSize }, default);
        await repository.Add(new() { Date = day, StartSlotIndex = targetPoint2, SlotCount = targetPointSize }, default);
        
        var service = new ScheduleService(repository, option.Object);

        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(day);


        // Assert
        var assuming = new List<ScheduleTimeWindow>
        {
            new(day, 0, 50),
            new(day, 70, 30)
        };
        Assert.Equal(assuming, positions);
    }
    
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_ZeroPositions()
    {
        // Arrange
        DateOnly day = DateOnly.MinValue;
        
        int slotsAmount = 100;
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        var repository = new ReservedTimeWindowMemoryRepository();

        var service = new ScheduleService(repository, option.Object);
        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(day);


        // Assert
        var assuming = new List<ScheduleTimeWindow>
        {
            new(day, 0, 100),
        };
        Assert.Equal(assuming, positions);
    }
}