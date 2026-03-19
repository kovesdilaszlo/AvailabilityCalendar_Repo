using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests additional edge cases for common free time calculations.
/// </summary>
public class AvailabilityServiceAdditionalEdgeCaseTests
{
    /// <summary>
    /// Verifies busy intervals are clipped to the requested range.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ClipBusyIntervalsToRange()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userId = Guid.NewGuid();
        var range = new TimeInterval(
            new DateTime(2026, 3, 22, 9, 0, 0),
            new DateTime(2026, 3, 22, 17, 0, 0));

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Starts before range",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 7, 0, 0),
            End = new DateTime(2026, 3, 22, 10, 0, 0)
        };

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Ends after range",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 15, 0, 0),
            End = new DateTime(2026, 3, 22, 19, 0, 0)
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<TimeInterval>()))
            .ReturnsAsync(new List<Event> { event1, event2 });

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { userId },
            range,
            TimeSpan.FromMinutes(1));

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 3, 22, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 3, 22, 15, 0, 0), result[0].End);
    }

    /// <summary>
    /// Verifies events completely outside the range are ignored.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_IgnoreIntervalsCompletelyOutsideRange()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userId = Guid.NewGuid();
        var range = new TimeInterval(
            new DateTime(2026, 3, 22, 9, 0, 0),
            new DateTime(2026, 3, 22, 17, 0, 0));

        var beforeEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Before range",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 6, 0, 0),
            End = new DateTime(2026, 3, 22, 8, 0, 0)
        };

        var afterEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "After range",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 18, 0, 0),
            End = new DateTime(2026, 3, 22, 19, 0, 0)
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<TimeInterval>()))
            .ReturnsAsync(new List<Event> { beforeEvent, afterEvent });

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { userId },
            range,
            TimeSpan.FromMinutes(1));

        // Assert
        Assert.Single(result);
        Assert.Equal(range.Start, result[0].Start);
        Assert.Equal(range.End, result[0].End);
    }

    /// <summary>
    /// Verifies unsorted busy intervals are handled correctly.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_HandleUnsortedBusyIntervals()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var userId = Guid.NewGuid();
        var range = new TimeInterval(
            new DateTime(2026, 3, 22, 9, 0, 0),
            new DateTime(2026, 3, 22, 17, 0, 0));

        var laterEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Later",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 13, 0, 0),
            End = new DateTime(2026, 3, 22, 14, 0, 0)
        };

        var earlierEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Earlier",
            CreatedByUserId = userId,
            Start = new DateTime(2026, 3, 22, 10, 0, 0),
            End = new DateTime(2026, 3, 22, 11, 0, 0)
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<TimeInterval>()))
            .ReturnsAsync(new List<Event> { laterEvent, earlierEvent });

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { userId },
            range,
            TimeSpan.FromMinutes(1));

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(new DateTime(2026, 3, 22, 9, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 3, 22, 10, 0, 0), result[0].End);

        Assert.Equal(new DateTime(2026, 3, 22, 11, 0, 0), result[1].Start);
        Assert.Equal(new DateTime(2026, 3, 22, 13, 0, 0), result[1].End);

        Assert.Equal(new DateTime(2026, 3, 22, 14, 0, 0), result[2].Start);
        Assert.Equal(new DateTime(2026, 3, 22, 17, 0, 0), result[2].End);
    }

    /// <summary>
    /// Verifies duplicate user IDs are removed before repository calls.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_DeduplicateUserIds_BeforeRepositoryCall()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new AvailabilityService(repoMock.Object);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var range = new TimeInterval(
            new DateTime(2026, 3, 22, 9, 0, 0),
            new DateTime(2026, 3, 22, 17, 0, 0));

        repoMock
            .Setup(r => r.GetByUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<TimeInterval>()))
            .ReturnsAsync(new List<Event>());

        // Act
        await service.GetCommonFreeTimeAsync(
            new List<Guid> { user1, user1, user2, user2 },
            range,
            TimeSpan.FromMinutes(1));

        // Assert
        repoMock.Verify(r => r.GetByUsersAsync(
            It.Is<List<Guid>>(ids =>
                ids.Count == 2 &&
                ids.Contains(user1) &&
                ids.Contains(user2)),
            range),
            Times.Once);
    }
}