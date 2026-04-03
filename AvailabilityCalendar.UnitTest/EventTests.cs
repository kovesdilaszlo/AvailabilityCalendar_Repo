using AvailabilityCalendar.Domain.Entities;
using Xunit;

namespace AvailabilityCalendar.Tests.DomainTests;

/// <summary>
/// Tests Event entity behavior.
/// </summary>
public class EventTests
{
    [Fact]
    public void AddParticipant_ShouldAddParticipant_WhenUserIsNotAlreadyPresent()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 3, 20, 10, 0, 0),
            End = new DateTime(2026, 3, 20, 11, 0, 0)
        };

        var userId = Guid.NewGuid();

        // Act
        ev.AddParticipant(userId);

        // Assert
        Assert.Single(ev.Participants);
        Assert.Contains(ev.Participants, p => p.UserId == userId);
    }

    [Fact]
    public void AddParticipant_ShouldNotDuplicateParticipant_WhenUserAlreadyPresent()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 3, 20, 10, 0, 0),
            End = new DateTime(2026, 3, 20, 11, 0, 0)
        };

        var userId = Guid.NewGuid();
        ev.AddParticipant(userId);

        // Act
        ev.AddParticipant(userId);

        // Assert
        Assert.Single(ev.Participants);
        Assert.Equal(1, ev.Participants.Count(p => p.UserId == userId));
    }

    [Fact]
    public void HasParticipant_ShouldReturnTrue_WhenUserIsParticipant()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting"
        };

        var userId = Guid.NewGuid();
        ev.AddParticipant(userId);

        // Act
        var result = ev.HasParticipant(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasParticipant_ShouldReturnFalse_WhenUserIsNotParticipant()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting"
        };

        ev.AddParticipant(Guid.NewGuid());
        var missingUserId = Guid.NewGuid();

        // Act
        var result = ev.HasParticipant(missingUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateTime_ShouldUpdateStartAndEnd()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting"
        };

        var newStart = new DateTime(2026, 3, 20, 13, 0, 0);
        var newEnd = new DateTime(2026, 3, 20, 14, 30, 0);

        // Act
        ev.UpdateTime(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, ev.Start);
        Assert.Equal(newEnd, ev.End);
    }

    [Fact]
    public void UpdateTime_ShouldThrow_WhenEndIsEarlierThanStart()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting"
        };

        var invalidStart = new DateTime(2026, 3, 20, 15, 0, 0);
        var invalidEnd = new DateTime(2026, 3, 20, 14, 0, 0);

        // Act
        var act = () => ev.UpdateTime(invalidStart, invalidEnd);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

}