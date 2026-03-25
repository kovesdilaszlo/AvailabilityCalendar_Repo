using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Repositories;

/// <summary>
/// Provides data access for User entities.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AvailabilityCalendarDbContext _dbContext;

    /// <summary>
    /// Initializes the repository with the database context.
    /// </summary>
    public UserRepository(AvailabilityCalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Gets users by a set of identifiers.
    /// </summary>
    public async Task<List<User>> GetByIdsAsync(List<Guid> ids)
    {
        ids ??= new List<Guid>();

        var normalizedIds = ids
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            return new List<User>();
        }

        return await _dbContext.DomainUsers
            .AsNoTracking()
            .Where(u => normalizedIds.Contains(u.Id))
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all users ordered by name.
    /// </summary>
    public async Task<List<User>> GetAllAsync()
    {
        return await _dbContext.DomainUsers
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}