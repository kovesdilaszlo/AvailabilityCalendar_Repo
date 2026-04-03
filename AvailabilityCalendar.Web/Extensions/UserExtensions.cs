using System.Security.Claims;

namespace AvailabilityCalendar.Web.Extensions;

/// <summary>
/// Extension methods for reading authenticated user information.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Gets the currently authenticated user's identifier from claims.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(rawId))
        {
            throw new InvalidOperationException("Authenticated user identifier is missing.");
        }

        if (!Guid.TryParse(rawId, out var userId))
        {
            throw new InvalidOperationException("Authenticated user identifier is not a valid Guid.");
        }

        return userId;
    }
}