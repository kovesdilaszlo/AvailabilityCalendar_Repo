using System.Timers;
using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests authorization rules enforced by EventService for updates and deletes.
/// </summary>
public class EventServiceAuthorizationTests
{
    /// <summary>
    /// Verifies deletion is blocked when the current user is not the creator.
    /// </summary>
    [Fact]
    public async Task DeleteEventAsync_ShouldThrow_WhenCurrentUserIsNotCreator()
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
            End = DateTime.Now.AddHours(1),
            CreatedByUserId = creatorId
        };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);

        var service = new EventService(repoMock.Object);

        // Act
        var action = async () => await service.DeleteEventAsync(eventId, currentUserId);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    /// <summary>
    /// Verifies deletion succeeds when the current user is the creator.
    /// </summary>
    [Fact]
    public async Task DeleteEventAsync_ShouldDelete_WhenCurrentUserIsCreator()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var ev = new Event
        {
            Id = eventId,
            Title = "Meeting",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1),
            CreatedByUserId = creatorId
        };

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);

        var service = new EventService(repoMock.Object);

        // Act
        await service.DeleteEventAsync(eventId, creatorId);

        // Assert
        repoMock.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }

    /// <summary>
    /// Verifies update is blocked when the current user is not the creator.
    /// </summary>
    [Fact]
    public async Task UpdateEventAsync_ShouldThrow_WhenCurrentUserIsNotCreator()
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
            End = DateTime.Now.AddHours(1),
            CreatedByUserId = creatorId
        };

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
    }
}