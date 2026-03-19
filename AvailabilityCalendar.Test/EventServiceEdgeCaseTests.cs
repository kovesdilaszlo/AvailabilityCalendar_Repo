using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests edge cases for deleting events through the EventService.
/// </summary>
public class EventServiceDeleteEdgeCaseTests
{
    /// <summary>
    /// Verifies deletion fails when the event does not exist.
    /// </summary>
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
}