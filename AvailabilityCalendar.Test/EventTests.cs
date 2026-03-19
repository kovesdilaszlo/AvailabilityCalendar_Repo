using Xunit;
using AvailabilityCalendar.Domain.Entities;

namespace AvailabilityCalendar.Tests.DomainTests;

/// <summary>
/// Unit tests for the Event domain entity behavior.
/// </summary>
public class EventTests
{
    /// <summary>
    /// Verifies AddParticipant adds a new participant when not already present.
    /// </summary>
    [Fact]
    public void AddParticipant_ShouldAddUser_WhenUserIsNotAlreadyParticipant()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = creatorId
        };

        // Act
        ev.AddParticipant(participantId);

        // Assert
        Assert.True(ev.HasParticipant(participantId));
        Assert.Single(ev.Participants);
    }

    /// <summary>
    /// Verifies AddParticipant does not duplicate a participant when already present.
    /// </summary>
    [Fact]
    public void AddParticipant_ShouldNotDuplicateUser_WhenUserAlreadyExists()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = creatorId
        };

        ev.AddParticipant(participantId);

        // Act
        ev.AddParticipant(participantId);

        // Assert
        Assert.Single(ev.Participants);
    }

    /// <summary>
    /// Verifies RemoveParticipant removes an existing participant.
    /// </summary>
    [Fact]
    public void RemoveParticipant_ShouldRemoveUser_WhenUserExists()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = creatorId
        };

        ev.AddParticipant(participantId);

        // Act
        ev.RemoveParticipant(participantId);

        // Assert
        Assert.False(ev.HasParticipant(participantId));
        Assert.Empty(ev.Participants);
    }

    /// <summary>
    /// Verifies HasParticipant returns false for non-participants.
    /// </summary>
    [Fact]
    public void HasParticipant_ShouldReturnFalse_WhenUserIsNotParticipant()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = creatorId
        };

        // Act
        var result = ev.HasParticipant(participantId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies UpdateTime updates start and end when the range is valid.
    /// </summary>
    [Fact]
    public void UpdateTime_ShouldUpdateStartAndEnd_WhenRangeIsValid()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = Guid.NewGuid()
        };

        var newStart = new DateTime(2026, 1, 1, 12, 0, 0);
        var newEnd = new DateTime(2026, 1, 1, 13, 30, 0);

        // Act
        ev.UpdateTime(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, ev.Start);
        Assert.Equal(newEnd, ev.End);
    }

    /// <summary>
    /// Verifies UpdateTime throws when end precedes start.
    /// </summary>
    [Fact]
    public void UpdateTime_ShouldThrow_WhenEndIsEarlierThanStart()
    {
        // Arrange
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = new DateTime(2026, 1, 1, 10, 0, 0),
            End = new DateTime(2026, 1, 1, 11, 0, 0),
            CreatedByUserId = Guid.NewGuid()
        };

        // Act
        var action = () => ev.UpdateTime(
            new DateTime(2026, 1, 1, 15, 0, 0),
            new DateTime(2026, 1, 1, 14, 0, 0));

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Verifies IsCreatedBy returns true when the user is the creator.
    /// </summary>
    [Fact]
    public void IsCreatedBy_ShouldReturnTrue_WhenUserIsCreator()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1),
            CreatedByUserId = creatorId
        };

        // Act
        var result = ev.IsCreatedBy(creatorId);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies IsCreatedBy returns false when the user is not the creator.
    /// </summary>
    [Fact]
    public void IsCreatedBy_ShouldReturnFalse_WhenUserIsNotCreator()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Meeting",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1),
            CreatedByUserId = creatorId
        };

        // Act
        var result = ev.IsCreatedBy(otherUserId);

        // Assert
        Assert.False(result);
    }
}