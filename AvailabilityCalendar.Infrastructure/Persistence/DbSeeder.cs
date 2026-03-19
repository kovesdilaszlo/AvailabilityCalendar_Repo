using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    /// <summary>
    /// Seeds identity users, domain users, and events when the database is empty.
    /// </summary>
    public static async Task SeedAsync(
        AvailabilityCalendarDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        await context.Database.MigrateAsync();

        var hasDomainUsers = await context.DomainUsers.AnyAsync();
        var hasIdentityUsers = await userManager.Users.AnyAsync();
        var hasEvents = await context.Events.AnyAsync();

        if (hasDomainUsers && hasIdentityUsers && hasEvents)
        {
            return;
        }

        var random = new Random();

        var users = new List<ApplicationUser>();

        if (!hasDomainUsers && !hasIdentityUsers)
        {
            for (int i = 1; i <= 10; i++)
            {
                var id = Guid.NewGuid();
                var email = $"user{i}@test.com";

                var identityUser = new ApplicationUser
                {
                    Id = id,
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(identityUser, "Password123!");

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"User seed hiba: {errors}");
                }

                users.Add(identityUser);

                var domainUser = new User
                {
                    Id = id,
                    Name = $"User {i}"
                };

                context.DomainUsers.Add(domainUser);
            }

            await context.SaveChangesAsync();
        }
        else
        {
            users = await userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        if (!hasEvents)
        {
            var events = new List<Event>();

            foreach (var user in users)
            {
                for (int j = 0; j < 30; j++)
                {
                    var dayOffset = random.Next(0, 30);
                    var hour = random.Next(8, 18);

                    var start = DateTime.Today.AddDays(dayOffset).AddHours(hour);
                    var duration = random.Next(1, 3);
                    var end = start.AddHours(duration);

                    var ev = new Event
                    {
                        Id = Guid.NewGuid(),
                        Title = $"Event {j + 1}",
                        Start = start,
                        End = end,
                        CreatedByUserId = user.Id,
                        Participants = new List<EventParticipant>
                        {
                            new EventParticipant
                            {
                                UserId = user.Id
                            }
                        }
                    };

                    events.Add(ev);
                }
            }

            context.Events.AddRange(events);
            await context.SaveChangesAsync();
        }
    }
}