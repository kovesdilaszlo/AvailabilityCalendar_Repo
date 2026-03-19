using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests core behaviors of EventService.CreateEventAsync.
/// </summary>
public class EventServiceCreateTests
{
    /// <summary>
    /// Verifies title trimming and participant deduplication during creation.
    /// </summary>
    [Fact]
    public async Task CreateEventAsync_Should_TrimTitle_And_AddCreatorAndDistinctParticipants()
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
        var otherUser1 = Guid.NewGuid();
        var otherUser2 = Guid.NewGuid();

        var command = new CreateEventCommand
        {
            Title = "   Team Meeting   ",
            Start = new DateTime(2026, 3, 20, 10, 0, 0),
            End = new DateTime(2026, 3, 20, 11, 0, 0),
            CurrentUserId = currentUserId,
            ParticipantIds = new List<Guid>
            {
                otherUser1,
                otherUser1,
                otherUser2,
                currentUserId
            }
        };

        // Act
        var createdId = await service.CreateEventAsync(command);

        // Assert
        repoMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);

        Assert.NotEqual(Guid.Empty, createdId);
        Assert.NotNull(savedEvent);
        Assert.Equal(createdId, savedEvent!.Id);

        Assert.Equal("Team Meeting", savedEvent.Title);
        Assert.Equal(currentUserId, savedEvent.CreatedByUserId);
        Assert.Equal(command.Start, savedEvent.Start);
        Assert.Equal(command.End, savedEvent.End);

        var participantIds = savedEvent.Participants
            .Select(p => p.UserId)
            .OrderBy(x => x)
            .ToList();

        var expected = new List<Guid> { currentUserId, otherUser1, otherUser2 }
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(expected, participantIds);
    }

    /// <summary>
    /// Verifies creation fails when the title is empty or whitespace.
    /// </summary>
    [Fact]
    public async Task CreateEventAsync_Should_Throw_When_TitleIsNullOrWhitespace()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var command = new CreateEventCommand
        {
            Title = "   ",
            Start = new DateTime(2026, 3, 20, 10, 0, 0),
            End = new DateTime(2026, 3, 20, 11, 0, 0),
            CurrentUserId = Guid.NewGuid(),
            ParticipantIds = new List<Guid>()
        };

        // Act
        var act = () => service.CreateEventAsync(command);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Event title cannot be empty.", ex.Message);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    /// <summary>
    /// Verifies creation fails when the end time precedes the start time.
    /// </summary>
    [Fact]
    public async Task CreateEventAsync_Should_Throw_When_EndIsEarlierThanStart()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var command = new CreateEventCommand
        {
            Title = "Invalid Event",
            Start = new DateTime(2026, 3, 20, 11, 0, 0),
            End = new DateTime(2026, 3, 20, 10, 0, 0),
            CurrentUserId = Guid.NewGuid(),
            ParticipantIds = new List<Guid>()
        };

        // Act
        var act = () => service.CreateEventAsync(command);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }
}