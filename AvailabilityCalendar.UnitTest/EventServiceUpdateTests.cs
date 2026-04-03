using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests event update behavior in EventService.
/// </summary>
public class EventServiceUpdateTests
{
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
            Title = "Old title"
        };
        existingEvent.AddParticipant(creatorId);
        existingEvent.UpdateTime(
            new DateTime(2026, 3, 21, 10, 0, 0),
            new DateTime(2026, 3, 21, 11, 0, 0));

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
            Title = "Old title"
        };
        existingEvent.AddParticipant(creatorId);
        existingEvent.UpdateTime(
            new DateTime(2026, 3, 21, 10, 0, 0),
            new DateTime(2026, 3, 21, 11, 0, 0));

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

    [Fact]
    public async Task UpdateEventAsync_Should_Throw_When_EndIsEarlierThanStart()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Old title"
        };
        existingEvent.AddParticipant(creatorId);
        existingEvent.UpdateTime(
            new DateTime(2026, 3, 21, 10, 0, 0),
            new DateTime(2026, 3, 21, 11, 0, 0));

        repoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        var command = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "Updated title",
            Start = new DateTime(2026, 3, 21, 15, 0, 0),
            End = new DateTime(2026, 3, 21, 14, 0, 0),
            CurrentUserId = creatorId
        };

        // Act
        var act = () => service.UpdateEventAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEventAsync_Should_Throw_WhenCurrentUserIsNotCreator()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var ev = new Event
        {
            Id = eventId,
            Title = "Meeting",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        };
        ev.AddParticipant(creatorId);

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);

        var service = new EventService(repoMock.Object);

        var command = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "Updated title",
            Start = DateTime.Now.AddHours(2),
            End = DateTime.Now.AddHours(3),
            CurrentUserId = currentUserId
        };

        // Act
        var action = async () => await service.UpdateEventAsync(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(action);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEventAsync_Should_Update_WhenCurrentUserIsCreator()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var ev = new Event
        {
            Id = eventId,
            Title = "Original title"
        };
        ev.AddParticipant(creatorId);
        ev.UpdateTime(
            new DateTime(2026, 3, 21, 9, 0, 0),
            new DateTime(2026, 3, 21, 10, 0, 0));

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);
        repoMock.Setup(r => r.UpdateAsync(ev))
            .Returns(Task.CompletedTask);

        var service = new EventService(repoMock.Object);

        var command = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "Updated title",
            Start = new DateTime(2026, 3, 21, 11, 0, 0),
            End = new DateTime(2026, 3, 21, 12, 0, 0),
            CurrentUserId = creatorId
        };

        // Act
        await service.UpdateEventAsync(command);

        // Assert
        Assert.Equal("Updated title", ev.Title);
        Assert.Equal(command.Start, ev.Start);
        Assert.Equal(command.End, ev.End);
        repoMock.Verify(r => r.UpdateAsync(ev), Times.Once);
    }
}