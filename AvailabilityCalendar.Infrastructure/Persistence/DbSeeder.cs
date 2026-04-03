using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds identity users, domain users, and sample events when the database is empty
/// or when required records are missing.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Applies migrations and inserts initial sample data if needed.
    /// Ensures that every identity user also has a matching domain user.
    /// </summary>
    public static async Task SeedAsync(
        AvailabilityCalendarDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        await context.Database.MigrateAsync();

        var random = new Random();

        // Load all existing identity users first.
        var identityUsers = await userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        // If there are no identity users yet, create sample users in both Identity and Domain.
        if (!identityUsers.Any())
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

                context.DomainUsers.Add(new User
                {
                    Id = id,
                    Name = $"User {i}"
                });
            }

            await context.SaveChangesAsync();

            identityUsers = await userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        // Ensure that every identity user has a matching domain user.
        var existingDomainUserIds = await context.DomainUsers
            .Select(u => u.Id)
            .ToListAsync();

        var missingDomainUsers = identityUsers
            .Where(identityUser => !existingDomainUserIds.Contains(identityUser.Id))
            .Select((identityUser, index) => new User
            {
                Id = identityUser.Id,
                Name = !string.IsNullOrWhiteSpace(identityUser.Email)
                    ? identityUser.Email
                    : $"User {index + 1}"
            })
            .ToList();

        if (missingDomainUsers.Any())
        {
            context.DomainUsers.AddRange(missingDomainUsers);
            await context.SaveChangesAsync();
        }

        var hasEvents = await context.Events.AnyAsync();

        // Seed sample events only if no events exist yet.
        if (!hasEvents)
        {
            var events = new List<Event>();

            foreach (var user in identityUsers)
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