using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests event deletion behavior in EventService.
/// </summary>
public class EventServiceDeleteTests
{
    [Fact]
    public async Task DeleteEventAsync_Should_Throw_When_EventNotFound()
    {
        // Arrange
        var repoMock = new Mock<IEventRepository>();
        var service = new EventService(repoMock.Object);

        var eventId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        repoMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var act = () => service.DeleteEventAsync(eventId, currentUserId);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("Event not found.", ex.Message);
        repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

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
            End = DateTime.Now.AddHours(1)
        };
        ev.AddParticipant(creatorId);

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);

        var service = new EventService(repoMock.Object);

        // Act
        var action = async () => await service.DeleteEventAsync(eventId, currentUserId);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(action);
        repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

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
            End = DateTime.Now.AddHours(1)
        };
        ev.AddParticipant(creatorId);

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(ev);

        var service = new EventService(repoMock.Object);

        // Act
        await service.DeleteEventAsync(eventId, creatorId);

        // Assert
        repoMock.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }
}