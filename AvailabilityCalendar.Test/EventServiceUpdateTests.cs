using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests for updating events through the EventService.
/// </summary>
public class EventServiceUpdateTests
{
    /// <summary>
    /// Verifies UpdateEventAsync trims the title, updates time, and persists changes.
    /// </summary>
    [Fact]
    public async Task UpdateEventAsync_Should_TrimTitle_UpdateTime_And_CallRepository()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Old title",
            CreatedByUserId = creatorId
        };
        existingEvent.UpdateTime(new DateTime(2026, 3, 21, 10, 0, 0), new DateTime(2026, 3, 21, 11, 0, 0));

        repoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        repoMock
            .Setup(r => r.UpdateAsync(existingEvent))
            .Returns(Task.CompletedTask);

        var command = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "   Updated title   ",
            Start = new DateTime(2026, 3, 21, 14, 0, 0),
            End = new DateTime(2026, 3, 21, 15, 30, 0),
            CurrentUserId = creatorId
        };

        // Act
        await service.UpdateEventAsync(command);

        // Assert
        Assert.Equal("Updated title", existingEvent.Title);
        Assert.Equal(command.Start, existingEvent.Start);
        Assert.Equal(command.End, existingEvent.End);

        repoMock.Verify(r => r.UpdateAsync(existingEvent), Times.Once);
    }

    /// <summary>
    /// Verifies UpdateEventAsync throws when the event cannot be found.
    /// </summary>
    [Fact]
    public async Task UpdateEventAsync_Should_Throw_When_EventNotFound()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var command = new UpdateEventCommand
        {
            EventId = Guid.NewGuid(),
            Title = "Updated title",
            Start = new DateTime(2026, 3, 21, 14, 0, 0),
            End = new DateTime(2026, 3, 21, 15, 0, 0),
            CurrentUserId = Guid.NewGuid()
        };

        repoMock
            .Setup(r => r.GetByIdAsync(command.EventId))
            .ReturnsAsync((Event?)null);

        // Act
        var act = () => service.UpdateEventAsync(command);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("Event not found.", ex.Message);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    /// <summary>
    /// Verifies UpdateEventAsync rejects empty or whitespace titles.
    /// </summary>
    [Fact]
    public async Task UpdateEventAsync_Should_Throw_When_TitleIsWhitespace()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Old title",
            CreatedByUserId = creatorId
        };
        existingEvent.UpdateTime(new DateTime(2026, 3, 21, 10, 0, 0), new DateTime(2026, 3, 21, 11, 0, 0));

        repoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        var command = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "   ",
            Start = new DateTime(2026, 3, 21, 14, 0, 0),
            End = new DateTime(2026, 3, 21, 15, 0, 0),
            CurrentUserId = creatorId
        };

        // Act
        var act = () => service.UpdateEventAsync(command);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Event title cannot be empty.", ex.Message);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }
}