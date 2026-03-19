using AvailabilityCalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.IntegrationTest;

public static class TestDbContextFactory
{
    public static AvailabilityCalendarDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AvailabilityCalendarDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AvailabilityCalendarDbContext(options);
    }
}