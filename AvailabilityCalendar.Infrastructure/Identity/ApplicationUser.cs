using Microsoft.AspNetCore.Identity;

namespace AvailabilityCalendar.Infrastructure.Identity;

/// <summary>
/// Application identity user with a Guid key.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}