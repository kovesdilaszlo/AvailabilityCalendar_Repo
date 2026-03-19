using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests edge cases for creating events through the EventService.
/// </summary>
public class EventServiceCreateEdgeCaseTests
{
    /// <summary>
    /// Verifies only the creator is added when no participant IDs are provided.
    /// </summary>
    [Fact]
    public async Task CreateEventAsync_Should_AddOnlyCreator_WhenParticipantIdsIsEmpty()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        Event? savedEvent = null;

        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Event>()))
            .Callback<Event>(e => savedEvent = e)
            .Returns(Task.CompletedTask);

        var service = new EventService(repoMock.Object);
        var currentUserId = Guid.NewGuid();

        var command = new CreateEventCommand
        {
            Title = "Solo event",
            Start = new DateTime(2026, 3, 23, 9, 0, 0),
            End = new DateTime(2026, 3, 23, 10, 0, 0),
            CurrentUserId = currentUserId,
            ParticipantIds = new List<Guid>()
        };

        // Act
        var createdId = await service.CreateEventAsync(command);

        // Assert
        Assert.NotEqual(Guid.Empty, createdId);
        Assert.NotNull(savedEvent);

        var participantIds = savedEvent!.Participants
            .Select(p => p.UserId)
            .ToList();

        Assert.Single(participantIds);
        Assert.Contains(currentUserId, participantIds);

        repoMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
    }

    /// <summary>
    /// Verifies creating an event fails when participant IDs are null.
    /// </summary>
    [Fact]
    public async Task CreateEventAsync_Should_Throw_WhenParticipantIdsIsNull()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var command = new CreateEventCommand
        {
            Title = "Null participants event",
            Start = new DateTime(2026, 3, 23, 9, 0, 0),
            End = new DateTime(2026, 3, 23, 10, 0, 0),
            CurrentUserId = Guid.NewGuid(),
            ParticipantIds = null!
        };

        // Act
        var act = () => service.CreateEventAsync(command);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(act);
        Assert.Equal("source", ex.ParamName);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }
}