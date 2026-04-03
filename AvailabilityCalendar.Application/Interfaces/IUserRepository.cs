using AvailabilityCalendar.Domain.Entities;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines persistence operations for user entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a single user by its identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets multiple users by their identifiers.
    /// </summary>
    Task<List<User>> GetByIdsAsync(List<Guid> ids);

    /// <summary>
    /// Gets all users ordered by name.
    /// </summary>
    Task<List<User>> GetAllAsync();
}