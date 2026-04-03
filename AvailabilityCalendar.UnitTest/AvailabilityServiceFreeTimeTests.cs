using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests free-time calculation behavior in AvailabilityService.
/// </summary>
public class AvailabilityServiceFreeTimeTests
{
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
    }

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

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnEmpty_WhenBusyIntervalsCoverWholeRange()
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
            new()
            {
                Id = Guid.NewGuid(),
                Title = "All day busy",
                Start = new DateTime(2026, 3, 24, 9, 0, 0),
                End = new DateTime(2026, 3, 24, 17, 0, 0)
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(busyEvents);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, TimeSpan.FromMinutes(1));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnGap_WhenGapMatchesMinimumDuration()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();

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
                End = new DateTime(2026, 1, 1, 11, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Afternoon busy",
                Start = new DateTime(2026, 1, 1, 13, 0, 0),
                End = new DateTime(2026, 1, 1, 17, 0, 0)
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[0].End);
    }

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_MergeOverlappingBusyEventsBeforeCalculatingFreeTime()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();

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
                End = new DateTime(2026, 1, 1, 12, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Busy 2",
                Start = new DateTime(2026, 1, 1, 11, 0, 0),
                End = new DateTime(2026, 1, 1, 14, 0, 0)
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 18, 0, 0), result[0].End);
    }

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
            Start = new DateTime(2026, 3, 22, 7, 0, 0),
            End = new DateTime(2026, 3, 22, 10, 0, 0)
        };

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Ends after range",
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
            Start = new DateTime(2026, 3, 22, 6, 0, 0),
            End = new DateTime(2026, 3, 22, 8, 0, 0)
        };

        var afterEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "After range",
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
            Start = new DateTime(2026, 3, 22, 13, 0, 0),
            End = new DateTime(2026, 3, 22, 14, 0, 0)
        };

        var earlierEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Earlier",
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

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_KeepInterval_WhenDurationIsExactlyMinimumDuration()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
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
                End = new DateTime(2026, 1, 1, 10, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Noon busy",
                Start = new DateTime(2026, 1, 1, 11, 0, 0),
                End = new DateTime(2026, 1, 1, 17, 0, 0)
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(repoMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), result[0].End);
    }

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_FilterOutIntervalsShorterThanMinimumDuration()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
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
                End = new DateTime(2026, 1, 1, 10, 0, 0)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Busy 2",
                Start = new DateTime(2026, 1, 1, 12, 0, 0),
                End = new DateTime(2026, 1, 1, 13, 0, 0)
            }
        };

        repoMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
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
}