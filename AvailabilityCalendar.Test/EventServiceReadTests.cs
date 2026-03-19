using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests reading events through the EventService.
/// </summary>
public class EventServiceReadTests
{
    /// <summary>
    /// Verifies only participant events are returned and sorted by start time.
    /// </summary>
    [Fact]
    public async Task GetEventsByUserAsync_Should_ReturnOnlyEventsWhereUserIsParticipant_OrderedByStart()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Later event",
            CreatedByUserId = creatorId
        };
        event1.UpdateTime(new DateTime(2026, 3, 21, 12, 0, 0), new DateTime(2026, 3, 21, 13, 0, 0));
        event1.AddParticipant(userId);

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Earlier event",
            CreatedByUserId = creatorId
        };
        event2.UpdateTime(new DateTime(2026, 3, 21, 9, 0, 0), new DateTime(2026, 3, 21, 10, 0, 0));
        event2.AddParticipant(userId);
        event2.AddParticipant(otherUserId);

        var event3 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Other user's event",
            CreatedByUserId = creatorId
        };
        event3.UpdateTime(new DateTime(2026, 3, 21, 8, 0, 0), new DateTime(2026, 3, 21, 9, 0, 0));
        event3.AddParticipant(otherUserId);

        repoMock
            .Setup(r => r.GetByUsersAsync(It.IsAny<List<Guid>>(), It.IsAny<TimeInterval>()))
            .ReturnsAsync(new List<Event> { event1, event2, event3 });

        var range = new TimeInterval(
            new DateTime(2026, 3, 21, 0, 0, 0),
            new DateTime(2026, 3, 22, 0, 0, 0));

        // Act
        var result = await service.GetEventsByUserAsync(userId, range);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Earlier event", result[0].Title);
        Assert.Equal("Later event", result[1].Title);

        Assert.All(result, dto => Assert.Contains(userId, dto.ParticipantIds));
        Assert.DoesNotContain(result, dto => dto.Title == "Other user's event");
    }
}