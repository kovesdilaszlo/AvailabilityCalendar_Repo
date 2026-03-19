using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests common free time calculation behaviors in AvailabilityService.
/// </summary>
public class AvailabilityServiceBehaviorTests
{
    /// <summary>
    /// Verifies the full range is returned when no users are specified and duration is sufficient.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnWholeRange_WhenUserListIsEmpty_AndRangeMeetsMinimumDuration()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userIds = new List<Guid>();
        var range = new TimeInterval(
            new DateTime(2026, 3, 24, 9, 0, 0),
            new DateTime(2026, 3, 24, 17, 0, 0));
        var minimumDuration = TimeSpan.FromHours(1);

        repoMock
            .Setup(r => r.GetByUsersAsync(It.Is<List<Guid>>(ids => ids.Count == 0), range))
            .ReturnsAsync(new List<Event>());

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(range.Start, result[0].Start);
        Assert.Equal(range.End, result[0].End);

        repoMock.Verify(r => r.GetByUsersAsync(
            It.Is<List<Guid>>(ids => ids.Count == 0),
            range),
            Times.Once);
    }

    /// <summary>
    /// Verifies no free time is returned when the minimum duration exceeds the range.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnEmpty_WhenMinimumDurationIsGreaterThanWholeFreeRange()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userIds = new List<Guid> { Guid.NewGuid() };
        var range = new TimeInterval(
            new DateTime(2026, 3, 24, 9, 0, 0),
            new DateTime(2026, 3, 24, 17, 0, 0));
        var minimumDuration = TimeSpan.FromHours(10);

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(new List<Event>());

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies no free time is returned when the range is fully occupied.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnEmpty_WhenBusyIntervalsCoverWholeRange_AndMinimumDurationWouldOtherwiseMatch()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userId = Guid.NewGuid();
        var userIds = new List<Guid> { userId };

        var range = new TimeInterval(
            new DateTime(2026, 3, 24, 9, 0, 0),
            new DateTime(2026, 3, 24, 17, 0, 0));

        var busyEvents = new List<Event>
        {
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "All day busy",
                Start = new DateTime(2026, 3, 24, 9, 0, 0),
                End = new DateTime(2026, 3, 24, 17, 0, 0),
                CreatedByUserId = userId
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(busyEvents);

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            userIds,
            range,
            TimeSpan.FromMinutes(1));

        // Assert
        Assert.Empty(result);
    }
}