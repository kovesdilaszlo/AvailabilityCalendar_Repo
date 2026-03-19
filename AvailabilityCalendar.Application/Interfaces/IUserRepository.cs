using AvailabilityCalendar.Domain.Entities;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Provides access to user data in the application layer.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetByIdsAsync(List<Guid> ids);
    Task<List<User>> GetAllAsync();
}