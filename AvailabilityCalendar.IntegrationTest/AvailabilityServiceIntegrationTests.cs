using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using AvailabilityCalendar.Infrastructure.Repositories;
using Xunit;

namespace AvailabilityCalendar.IntegrationTest;

public class AvailabilityServiceIntegrationTests
{
    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ReturnCorrectFreeSlots_WithRealDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new AvailabilityService(repo);

        var user = Guid.NewGuid();

        var busyEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Busy"
        };
        busyEvent.UpdateTime(
            new DateTime(2026, 3, 26, 10, 0, 0),
            new DateTime(2026, 3, 26, 11, 0, 0));
        busyEvent.AddParticipant(user);

        context.Events.Add(busyEvent);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 9, 0, 0),
            new DateTime(2026, 3, 26, 12, 0, 0));

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { user },
            range,
            TimeSpan.FromMinutes(30));

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(new DateTime(2026, 3, 26, 9, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 10, 0, 0), result[0].End);

        Assert.Equal(new DateTime(2026, 3, 26, 11, 0, 0), result[1].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 12, 0, 0), result[1].End);
    }

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_WorkWithMultipleUsers_AndMergedBusyIntervals()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new AvailabilityService(repo);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User1 busy"
        };
        event1.UpdateTime(
            new DateTime(2026, 3, 26, 9, 0, 0),
            new DateTime(2026, 3, 26, 10, 30, 0));
        event1.AddParticipant(user1);

        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User2 busy"
        };
        event2.UpdateTime(
            new DateTime(2026, 3, 26, 10, 0, 0),
            new DateTime(2026, 3, 26, 11, 0, 0));
        event2.AddParticipant(user2);

        var event3 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "User2 later busy"
        };
        event3.UpdateTime(
            new DateTime(2026, 3, 26, 13, 0, 0),
            new DateTime(2026, 3, 26, 14, 0, 0));
        event3.AddParticipant(user2);

        context.Events.AddRange(event1, event2, event3);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 8, 0, 0),
            new DateTime(2026, 3, 26, 16, 0, 0));

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { user1, user2 },
            range,
            TimeSpan.FromMinutes(30));

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(new DateTime(2026, 3, 26, 8, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 9, 0, 0), result[0].End);

        Assert.Equal(new DateTime(2026, 3, 26, 11, 0, 0), result[1].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 13, 0, 0), result[1].End);

        Assert.Equal(new DateTime(2026, 3, 26, 14, 0, 0), result[2].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 16, 0, 0), result[2].End);
    }

    [Fact]
    public async Task GetCommonFreeTimeAsync_Should_ClipEventsToRange_WithRealDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repo = new EventRepository(context);
        var service = new AvailabilityService(repo);

        var user = Guid.NewGuid();

        var eventBeforeRange = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Starts before range"
        };
        eventBeforeRange.UpdateTime(
            new DateTime(2026, 3, 26, 7, 0, 0),
            new DateTime(2026, 3, 26, 10, 0, 0));
        eventBeforeRange.AddParticipant(user);

        var eventAfterRange = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Ends after range"
        };
        eventAfterRange.UpdateTime(
            new DateTime(2026, 3, 26, 15, 0, 0),
            new DateTime(2026, 3, 26, 19, 0, 0));
        eventAfterRange.AddParticipant(user);

        context.Events.AddRange(eventBeforeRange, eventAfterRange);
        await context.SaveChangesAsync();

        var range = new TimeInterval(
            new DateTime(2026, 3, 26, 9, 0, 0),
            new DateTime(2026, 3, 26, 17, 0, 0));

        // Act
        var result = await service.GetCommonFreeTimeAsync(
            new List<Guid> { user },
            range,
            TimeSpan.FromMinutes(30));

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 3, 26, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 3, 26, 15, 0, 0), result[0].End);
    }
}