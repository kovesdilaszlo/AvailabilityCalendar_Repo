using Xunit;
using Moq;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Test;

public class GetCommonFreeTimeTests
{
    /// <summary>
    /// Verifies full range is returned when no events exist and duration meets the minimum.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldReturnWholeRange_WhenUsersHaveNoEvents_AndRangeMatchesMinimumDuration()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 17, 0, 0));

        var minimumDuration = TimeSpan.FromHours(8);

        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(new List<Event>());

        var service = new AvailabilityService(eventRepositoryMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Single(result);
        Assert.Equal(range.Start, result[0].Start);
        Assert.Equal(range.End, result[0].End);
    }

    /// <summary>
    /// Verifies no free time is returned when the range is shorter than the minimum duration.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldReturnEmpty_WhenUsersHaveNoEvents_ButRangeIsShorterThanMinimumDuration()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 9, 30, 0));

        var minimumDuration = TimeSpan.FromHours(1);

        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(new List<Event>());

        var service = new AvailabilityService(eventRepositoryMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies no free time is returned when users are busy for the entire range.
    /// </summary>
    [Fact]
    public async Task GetCommonFreeTimeAsync_ShouldReturnEmpty_WhenUsersAreBusyForWholeRange()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var userIds = new List<Guid> { user1, user2 };

        var range = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 17, 0, 0));

        var minimumDuration = TimeSpan.FromMinutes(30);

        var events = new List<Event>
        {
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Busy",
                Start = new DateTime(2026, 1, 1, 9, 0, 0),
                End = new DateTime(2026, 1, 1, 17, 0, 0),
                CreatedByUserId = user1
            }
        };

        var eventRepositoryMock = new Mock<IEventRepository>();
        eventRepositoryMock
            .Setup(r => r.GetByUsersAsync(userIds, range))
            .ReturnsAsync(events);

        var service = new AvailabilityService(eventRepositoryMock.Object);

        // Act
        var result = await service.GetCommonFreeTimeAsync(userIds, range, minimumDuration);

        // Assert
        Assert.Empty(result);
    }
}