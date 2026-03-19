using Xunit;
using Moq;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Test;

public class GetCommonFreeTimeEdgeCaseTests
{
    /// <summary>
    /// Verifies the free gap is returned when it matches the minimum duration.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldReturnGapBetweenBusyIntervals_WhenGapMatchesMinimumDuration()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 17, 0, 0));

        var minimumDuration = TimeSpan.FromHours(2);

        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Morning busy",
                Start = new DateTime(2026, 1, 1, 9, 0, 0),
                End = new DateTime(2026, 1, 1, 11, 0, 0),
                CreatedByUserId = userIds[0]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Afternoon busy",
                Start = new DateTime(2026, 1, 1, 13, 0, 0),
                End = new DateTime(2026, 1, 1, 17, 0, 0),
                CreatedByUserId = userIds[1]
            }
        };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[0].End);
    }

    /// <summary>
    /// Verifies shorter gaps are filtered out based on the minimum duration.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldFilterOutIntervalsShorterThanMinimumDuration()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 8, 0, 0),
            new DateTime(2026, 1, 1, 18, 0, 0));

        var minimumDuration = TimeSpan.FromHours(2);

        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Busy 1",
                Start = new DateTime(2026, 1, 1, 9, 0, 0),
                End = new DateTime(2026, 1, 1, 10, 0, 0),
                CreatedByUserId = userIds[0]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Busy 2",
                Start = new DateTime(2026, 1, 1, 12, 0, 0),
                End = new DateTime(2026, 1, 1, 13, 0, 0),
                CreatedByUserId = userIds[1]
            }
        };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 0, 0), result[0].End);

        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[1].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 18, 0, 0), result[1].End);
    }

    /// <summary>
    /// Verifies intervals are kept when duration matches the minimum.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldKeepInterval_WhenDurationIsExactlyMinimumDuration()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 17, 0, 0));

        var minimumDuration = TimeSpan.FromHours(1);

        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Morning busy",
                Start = new DateTime(2026, 1, 1, 9, 0, 0),
                End = new DateTime(2026, 1, 1, 10, 0, 0),
                CreatedByUserId = userIds[0]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Noon busy",
                Start = new DateTime(2026, 1, 1, 11, 0, 0),
                End = new DateTime(2026, 1, 1, 17, 0, 0),
                CreatedByUserId = userIds[1]
            }
        };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), result[0].End);
    }

    /// <summary>
    /// Verifies overlapping busy events are merged before calculating free time.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldMergeOverlappingBusyEventsBeforeCalculatingFreeTime_AndThenFilter()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 8, 0, 0),
            new DateTime(2026, 1, 1, 18, 0, 0));

        var minimumDuration = TimeSpan.FromHours(2);

        var events = new List<Event>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Busy 1",
            Start = new DateTime(2026, 1, 1, 9, 0, 0),
            End = new DateTime(2026, 1, 1, 12, 0, 0),
            CreatedByUserId = userIds[0]
        },
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Busy 2",
            Start = new DateTime(2026, 1, 1, 11, 0, 0),
            End = new DateTime(2026, 1, 1, 14, 0, 0),
            CreatedByUserId = userIds[1]
        }
    };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);

        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 18, 0, 0), result[0].End);
    }
}