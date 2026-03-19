using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines event management operations.
/// </summary>
public interface IEventService
{
    Task<Guid> CreateEventAsync(CreateEventCommand command);
    Task UpdateEventAsync(UpdateEventCommand command);
    Task DeleteEventAsync(Guid eventId, Guid currentUserId);
    Task<List<EventDto>> GetEventsByUserAsync(Guid userId, TimeInterval range);
}