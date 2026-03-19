using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Provides access to event data in the application layer.
/// </summary>
public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetByUsersAsync(List<Guid> userIds, TimeInterval range);
    Task AddAsync(Event entity);
    Task UpdateAsync(Event entity);
    Task DeleteAsync(Guid id);
}