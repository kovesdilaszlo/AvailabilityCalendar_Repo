using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines application-level operations for event management.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new event and returns its identifier.
    /// </summary>
    Task<Guid> CreateEventAsync(CreateEventCommand command);

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    Task UpdateEventAsync(UpdateEventCommand command);

    /// <summary>
    /// Deletes an existing event.
    /// </summary>
    Task DeleteEventAsync(Guid eventId, Guid currentUserId);

    /// <summary>
    /// Gets all events visible to the specified user within the given range.
    /// </summary>
    Task<List<EventDto>> GetEventsByUserAsync(Guid userId, TimeInterval range);
}