using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Data;

namespace ServicesApiTests;

public class SchedulingTests
{
    private ServicesDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ServicesDbContext(options);
    }
    
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_OnePosition()
    {
        // Arrange
        DateOnly day = DateOnly.MinValue;
        
        int slotsAmount = 100;
        int targetPoint = 50;
        int targetPointSize = 10;
        var doctorId = Guid.NewGuid();
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        await using var context = CreateInMemoryDbContext();
        var repository = new ReservedTimeWindowStore(context, null);

        await repository.TryAdd(new() { DoctorId = doctorId, Date = day, StartSlotIndex = targetPoint, SlotCount = targetPointSize }, default);
        
        var service = new ReservationService(repository, option.Object);

        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(doctorId, day, default);


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
        var doctorId = Guid.NewGuid();
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        await using var context = CreateInMemoryDbContext();
        var repository = new ReservedTimeWindowStore(context, null);

        await repository.TryAdd(new() {DoctorId = doctorId, Date = day, StartSlotIndex = targetPoint, SlotCount = targetPointSize }, default);
        await repository.TryAdd(new() {DoctorId = doctorId,  Date = day, StartSlotIndex = targetPoint2, SlotCount = targetPointSize }, default);
        
        var service = new ReservationService(repository, option.Object);

        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(doctorId, day, default);


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
        var doctorId = Guid.NewGuid();
        var option = new Mock<IScheduleSlotsProvider>();
        option.Setup(x => x.GetSlotsAmount()).Returns(slotsAmount);
        
        await using var context = CreateInMemoryDbContext();
        var repository = new ReservedTimeWindowStore(context, null);

        var service = new ReservationService(repository, option.Object);
        
        // Act
        var positions = await service.GetAvailablePositionsOnDay(doctorId, day, default);


        // Assert
        var assuming = new List<ScheduleTimeWindow>
        {
            new(day, 0, 100),
        };
        Assert.Equal(assuming, positions);
    }
}
