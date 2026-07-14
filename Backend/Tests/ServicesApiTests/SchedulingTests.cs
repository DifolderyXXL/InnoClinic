using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesApiTests;

public class SchedulingTests
{
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_OnePosition()
    {
        // Arrange
        var day = DateOnly.FromDateTime(DateTime.Today);
        var reserved = new List<ReservedTimeWindow> 
        { 
            new() { StartSlotIndex = 50, SlotCount = 10, Date = day } 
        };

        // Act
        var result = ScheduleCalculator.CalculateAvailableGaps(day, reserved, totalSlotsAmount: 100).ToList();

        // Assert
        var expected = new List<ScheduleTimeWindow>
        {
            new(day, 0, 50),
            new(day, 60, 40)
        };
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_TwoPositionInRow()
    {
        // Arrange
        var day = DateOnly.FromDateTime(DateTime.Today);
        var reserved = new List<ReservedTimeWindow> 
        { 
            new() { StartSlotIndex = 50, SlotCount = 10, Date = day },
            new() { StartSlotIndex = 60, SlotCount = 10, Date = day },
        };

        // Act
        var result = ScheduleCalculator.CalculateAvailableGaps(day, reserved, totalSlotsAmount: 100).ToList();

        // Assert
        var expected = new List<ScheduleTimeWindow>
        {
            new(day, 0, 50),
            new(day, 70, 30)
        };
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task ScheduleService_GetAvailablePositionsOnDay_ZeroPositions()
    {
        // Arrange
        var day = DateOnly.FromDateTime(DateTime.Today);
        var reserved = new List<ReservedTimeWindow>();

        // Act
        var result = ScheduleCalculator.CalculateAvailableGaps(day, reserved, totalSlotsAmount: 100).ToList();

        // Assert
        var expected = new List<ScheduleTimeWindow>
        {
            new(day, 0, 100),
        };
        Assert.Equal(expected, result);
    }
}
