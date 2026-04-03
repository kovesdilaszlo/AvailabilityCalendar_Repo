using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines persistence operations for event entities.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Gets a single event by its identifier.
    /// </summary>
    Task<Event?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all events that belong to any of the specified users
    /// within the given time range.
    /// </summary>
    Task<List<Event>> GetByUsersAsync(List<Guid> userIds, TimeInterval range);

    /// <summary>
    /// Persists a newly created event.
    /// </summary>
    Task AddAsync(Event entity);

    /// <summary>
    /// Persists changes made to an existing event.
    /// </summary>
    Task UpdateAsync(Event entity);

    /// <summary>
    /// Deletes an event by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);
}