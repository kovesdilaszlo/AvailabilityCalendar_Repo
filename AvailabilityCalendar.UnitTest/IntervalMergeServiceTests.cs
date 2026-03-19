using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests interval merging behavior in AvailabilityService.
/// </summary>
public class IntervalMergeServiceTests
{
    [Fact]
    public void MergeIntervals_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>();

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MergeIntervals_ShouldReturnSameSingleInterval_WhenInputContainsOneInterval()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var interval = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        // Act
        var result = service.MergeIntervals(new List<TimeInterval> { interval });

        // Assert
        Assert.Single(result);
        Assert.Equal(interval.Start, result[0].Start);
        Assert.Equal(interval.End, result[0].End);
    }

    [Fact]
    public void MergeIntervals_ShouldMergeOverlappingIntervals()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 11, 0, 0), new DateTime(2026, 1, 1, 13, 0, 0))
        };

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[0].End);
    }

    [Fact]
    public void MergeIntervals_ShouldMergeMultipleChainedOverlaps()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 11, 30, 0), new DateTime(2026, 1, 1, 13, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 45, 0), new DateTime(2026, 1, 1, 14, 0, 0))
        };

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].End);
    }

    [Fact]
    public void MergeIntervals_ShouldKeepSeparateIntervals_WhenTheyDoNotOverlap()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 11, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 0, 0), new DateTime(2026, 1, 1, 13, 0, 0))
        };

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), result[0].End);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 0, 0), result[1].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[1].End);
    }

    [Fact]
    public void MergeIntervals_ShouldMergeTouchingIntervals()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 0, 0), new DateTime(2026, 1, 1, 14, 0, 0))
        };

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].End);
    }

    [Fact]
    public void MergeIntervals_ShouldHandleUnsortedInput()
    {
        // Arrange
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 13, 0, 0), new DateTime(2026, 1, 1, 14, 0, 0)),
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 11, 30, 0), new DateTime(2026, 1, 1, 13, 0, 0))
        };

        // Act
        var result = service.MergeIntervals(intervals);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].End);
    }
}