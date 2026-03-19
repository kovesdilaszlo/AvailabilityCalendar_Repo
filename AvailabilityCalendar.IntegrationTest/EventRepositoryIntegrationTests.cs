using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using AvailabilityCalendar.Infrastructure.Repositories;
using Xunit;

namespace AvailabilityCalendar.IntegrationTest;

public class EventRepositoryIntegrationTests
{
    [Fact]
    public async Task GetByUsersAsync_Should_FilterByParticipant_And_TimeRange()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User1 Event",
            CreatedByUserId = user1
        };
        event1.UpdateTime(
            new DateTime(2026, 3, 26, 10, 0, 0),
            new DateTime(2026, 3, 26, 11, 0, 0));
        event1.AddParticipant(user1);

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User2 Event",
            CreatedByUserId = user2
        };
        event2.UpdateTime(
            new DateTime(2026, 3, 26, 12, 0, 0),
            new DateTime(2026, 3, 26, 13, 0, 0));
        event2.AddParticipant(user2);

        var outsideRangeEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Outside Range",
            CreatedByUserId = user1
        };
        outsideRangeEvent.UpdateTime(
            new DateTime(2026, 3, 27, 10, 0, 0),
            new DateTime(2026, 3, 27, 11, 0, 0));
        outsideRangeEvent.AddParticipant(user1);

        context.Events.AddRange(event1, event2, outsideRangeEvent);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 0, 0, 0),
            new DateTime(2026, 3, 27, 0, 0, 0));

        // Act
        var result = await repo.GetByUsersAsync(new List<Guid> { user1 }, range);

        // Assert
        Assert.Single(result);
        Assert.Equal("User1 Event", result[0].Title);
        Assert.Contains(result[0].Participants, p => p.UserId == user1);
    }

    [Fact]
    public async Task GetByUsersAsync_Should_ReturnEvents_ForAllRequestedUsers()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User1 Event",
            CreatedByUserId = user1
        };
        event1.UpdateTime(
            new DateTime(2026, 3, 26, 9, 0, 0),
            new DateTime(2026, 3, 26, 10, 0, 0));
        event1.AddParticipant(user1);

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User2 Event",
            CreatedByUserId = user2
        };
        event2.UpdateTime(
            new DateTime(2026, 3, 26, 11, 0, 0),
            new DateTime(2026, 3, 26, 12, 0, 0));
        event2.AddParticipant(user2);

        var event3 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User3 Event",
            CreatedByUserId = user3
        };
        event3.UpdateTime(
            new DateTime(2026, 3, 26, 13, 0, 0),
            new DateTime(2026, 3, 26, 14, 0, 0));
        event3.AddParticipant(user3);

        context.Events.AddRange(event1, event2, event3);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 0, 0, 0),
            new DateTime(2026, 3, 27, 0, 0, 0));

        // Act
        var result = await repo.GetByUsersAsync(new List<Guid> { user1, user2 }, range);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Title == "User1 Event");
        Assert.Contains(result, e => e.Title == "User2 Event");
        Assert.DoesNotContain(result, e => e.Title == "User3 Event");
    }

    [Fact]
    public async Task GetByUsersAsync_Should_ReturnEvent_WhenItPartiallyOverlapsRange()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);

        var user = Guid.NewGuid();

        var overlappingEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Partially overlapping event",
            CreatedByUserId = user
        };
        overlappingEvent.UpdateTime(
            new DateTime(2026, 3, 26, 8, 30, 0),
            new DateTime(2026, 3, 26, 9, 30, 0));
        overlappingEvent.AddParticipant(user);

        context.Events.Add(overlappingEvent);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 9, 0, 0),
            new DateTime(2026, 3, 26, 10, 0, 0));

        // Act
        var result = await repo.GetByUsersAsync(new List<Guid> { user }, range);

        // Assert
        Assert.Single(result);
        Assert.Equal("Partially overlapping event", result[0].Title);
    }

    [Fact]
    public async Task GetByUsersAsync_Should_ReturnEmpty_WhenNoMatchingEventsExist()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);

        var user = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var unrelatedEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Unrelated",
            CreatedByUserId = otherUser
        };
        unrelatedEvent.UpdateTime(
            new DateTime(2026, 3, 26, 10, 0, 0),
            new DateTime(2026, 3, 26, 11, 0, 0));
        unrelatedEvent.AddParticipant(otherUser);

        context.Events.Add(unrelatedEvent);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 0, 0, 0),
            new DateTime(2026, 3, 27, 0, 0, 0));

        // Act
        var result = await repo.GetByUsersAsync(new List<Guid> { user }, range);

        // Assert
        Assert.Empty(result);
    }
}