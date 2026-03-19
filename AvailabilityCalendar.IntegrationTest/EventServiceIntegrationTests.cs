using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.ValueObjects;
using AvailabilityCalendar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AvailabilityCalendar.IntegrationTest;

public class EventServiceIntegrationTests
{
    [Fact]
    public async Task CreateEventAsync_Should_PersistEvent_WithCreatorAndParticipants()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new EventService(repo);

        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var command = new CreateEventCommand
        {
            Title = "Integration Event",
            Start = new DateTime(2026, 3, 26, 10, 0, 0),
            End = new DateTime(2026, 3, 26, 11, 0, 0),
            CurrentUserId = creator,
            ParticipantIds = new List<Guid> { otherUser }
        };

        // Act
        var id = await service.CreateEventAsync(command);

        // Assert
        var saved = await context.Events
            .Include(e => e.Participants)
            .SingleAsync(e => e.Id == id);

        Assert.Equal("Integration Event", saved.Title);
        Assert.Equal(creator, saved.CreatedByUserId);

        var participants = saved.Participants.Select(p => p.UserId).ToList();

        Assert.Contains(creator, participants);
        Assert.Contains(otherUser, participants);
    }

    [Fact]
    public async Task CreateEventAsync_Should_TrimTitle_And_DeduplicateParticipants_InDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new EventService(repo);

        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var command = new CreateEventCommand
        {
            Title = "   Integration Event   ",
            Start = new DateTime(2026, 3, 26, 10, 0, 0),
            End = new DateTime(2026, 3, 26, 11, 0, 0),
            CurrentUserId = creator,
            ParticipantIds = new List<Guid> { otherUser, otherUser, creator }
        };

        // Act
        var id = await service.CreateEventAsync(command);

        // Assert
        var saved = await context.Events
            .Include(e => e.Participants)
            .SingleAsync(e => e.Id == id);

        Assert.Equal("Integration Event", saved.Title);

        var participants = saved.Participants
            .Select(p => p.UserId)
            .OrderBy(x => x)
            .ToList();

        var expected = new List<Guid> { creator, otherUser }
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(expected, participants);
    }

    [Fact]
    public async Task FullFlow_Should_Create_Read_Update_Delete_Event()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new EventService(repo);

        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var createCommand = new CreateEventCommand
        {
            Title = "Flow Event",
            Start = new DateTime(2026, 3, 27, 9, 0, 0),
            End = new DateTime(2026, 3, 27, 10, 0, 0),
            CurrentUserId = creator,
            ParticipantIds = new List<Guid> { otherUser }
        };

        // Act - create
        var eventId = await service.CreateEventAsync(createCommand);

        // Assert - read after create
        var initialRange = new TimeInterval(
            new DateTime(2026, 3, 27, 0, 0, 0),
            new DateTime(2026, 3, 28, 0, 0, 0));

        var createdEvents = await service.GetEventsByUserAsync(creator, initialRange);

        Assert.Contains(createdEvents, e => e.Id == eventId && e.Title == "Flow Event");

        // Act - update
        var updateCommand = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "Updated Flow Event",
            Start = new DateTime(2026, 3, 27, 11, 0, 0),
            End = new DateTime(2026, 3, 27, 12, 0, 0),
            CurrentUserId = creator
        };

        await service.UpdateEventAsync(updateCommand);

        // Assert - read after update
        var updatedEvents = await service.GetEventsByUserAsync(creator, initialRange);
        var updated = updatedEvents.Single(e => e.Id == eventId);

        Assert.Equal("Updated Flow Event", updated.Title);
        Assert.Equal(updateCommand.Start, updated.Start);
        Assert.Equal(updateCommand.End, updated.End);

        // Act - delete
        await service.DeleteEventAsync(eventId, creator);

        // Assert - read after delete
        var afterDelete = await service.GetEventsByUserAsync(creator, initialRange);
        Assert.DoesNotContain(afterDelete, e => e.Id == eventId);

        var existsInDb = await context.Events.AnyAsync(e => e.Id == eventId);
        Assert.False(existsInDb);
    }

    [Fact]
    public async Task UpdateEventAsync_Should_Throw_WhenUserIsNotCreator_WithRealDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new EventService(repo);

        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var createCommand = new CreateEventCommand
        {
            Title = "Protected Event",
            Start = new DateTime(2026, 3, 27, 9, 0, 0),
            End = new DateTime(2026, 3, 27, 10, 0, 0),
            CurrentUserId = creator,
            ParticipantIds = new List<Guid> { otherUser }
        };

        var eventId = await service.CreateEventAsync(createCommand);

        var updateCommand = new UpdateEventCommand
        {
            EventId = eventId,
            Title = "Hacked Title",
            Start = new DateTime(2026, 3, 27, 11, 0, 0),
            End = new DateTime(2026, 3, 27, 12, 0, 0),
            CurrentUserId = otherUser
        };

        // Act
        var act = () => service.UpdateEventAsync(updateCommand);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

        var saved = await context.Events.SingleAsync(e => e.Id == eventId);
        Assert.Equal("Protected Event", saved.Title);
    }

    [Fact]
    public async Task DeleteEventAsync_Should_Throw_WhenUserIsNotCreator_WithRealDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new EventService(repo);

        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var createCommand = new CreateEventCommand
        {
            Title = "Protected Event",
            Start = new DateTime(2026, 3, 27, 13, 0, 0),
            End = new DateTime(2026, 3, 27, 14, 0, 0),
            CurrentUserId = creator,
            ParticipantIds = new List<Guid> { otherUser }
        };

        var eventId = await service.CreateEventAsync(createCommand);

        // Act
        var act = () => service.DeleteEventAsync(eventId, otherUser);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        Assert.True(await context.Events.AnyAsync(e => e.Id == eventId));
    }
}