using Microsoft.AspNetCore.Identity;

namespace AvailabilityCalendar.Infrastructure.Identity;

/// <summary>
/// Identity user used for authentication and authorization.
/// A Guid is used as the primary key type.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}