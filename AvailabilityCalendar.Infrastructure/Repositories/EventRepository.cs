using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;
using AvailabilityCalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Repositories;

/// <summary>
/// Provides data access for Event entities.
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly AvailabilityCalendarDbContext _dbContext;

    /// <summary>
    /// Initializes the repository with the database context.
    /// </summary>
    public EventRepository(AvailabilityCalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets an event by its identifier, including participants.
    /// </summary>
    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Gets events that include any of the specified users within a time range.
    /// </summary>
    public async Task<List<Event>> GetByUsersAsync(List<Guid> userIds, TimeInterval range)
    {
        userIds ??= new List<Guid>();

        var normalizedUserIds = userIds
            .Distinct()
            .ToList();

        if (normalizedUserIds.Count == 0)
        {
            return new List<Event>();
        }

        return await _dbContext.Events
            .AsNoTracking()
            .Include(e => e.Participants)
            .Where(e =>
                e.Participants.Any(p => normalizedUserIds.Contains(p.UserId)) &&
                e.End > range.Start &&
                e.Start < range.End)
            .OrderBy(e => e.Start)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new event and persists changes.
    /// </summary>
    public async Task AddAsync(Event entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbContext.Events.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing event and persists changes.
    /// </summary>
    public async Task UpdateAsync(Event entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var existing = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        if (existing is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        existing.Title = entity.Title;
        existing.Start = entity.Start;
        existing.End = entity.End;
        existing.CreatedByUserId = entity.CreatedByUserId;

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an event by identifier if it exists.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null)
        {
            return;
        }

        _dbContext.Events.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}