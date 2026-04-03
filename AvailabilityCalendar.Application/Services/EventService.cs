using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Services;

/// <summary>
/// Service responsible for creating, reading, updating and deleting events.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    /// <summary>
    /// Creates a new event service instance.
    /// </summary>
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Creates a new event and automatically adds the current user
    /// to the participant list.
    /// </summary>
    public async Task<Guid> CreateEventAsync(CreateEventCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ArgumentException("Event title cannot be empty.");
        }

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim()
        };

        ev.UpdateTime(command.Start, command.End);

        // The current user is always included as a participant.
        ev.AddParticipant(command.CurrentUserId);

        // Any additionally selected users are also attached to the event.
        foreach (var participantId in command.ParticipantIds.Distinct())
        {
            ev.AddParticipant(participantId);
        }

        await _eventRepository.AddAsync(ev);

        return ev.Id;
    }

    /// <summary>
    /// Gets all events associated with the specified user
    /// within the given time range.
    /// </summary>
    public async Task<List<EventDto>> GetEventsByUserAsync(Guid userId, TimeInterval range)
    {
        var events = await _eventRepository.GetByUsersAsync(new List<Guid> { userId }, range);

        return events
            .Where(e => e.HasParticipant(userId))
            .OrderBy(e => e.Start)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Start = e.Start,
                End = e.End,
                ParticipantIds = e.Participants
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList()
            })
            .ToList();
    }

    /// <summary>
    /// Updates an existing event.
    /// Only participants of the event are allowed to modify it
    /// in the simplified collaborative model.
    /// </summary>
    public async Task UpdateEventAsync(UpdateEventCommand command)
    {
        var ev = await _eventRepository.GetByIdAsync(command.EventId);

        if (ev is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        if (!ev.HasParticipant(command.CurrentUserId))
        {
            throw new UnauthorizedAccessException("Only a participant can update the event.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ArgumentException("Event title cannot be empty.");
        }

        ev.Title = command.Title.Trim();
        ev.UpdateTime(command.Start, command.End);

        await _eventRepository.UpdateAsync(ev);
    }

    /// <summary>
    /// Deletes an existing event.
    /// Only participants of the event are allowed to delete it
    /// in the simplified collaborative model.
    /// </summary>
    public async Task DeleteEventAsync(Guid eventId, Guid currentUserId)
    {
        var ev = await _eventRepository.GetByIdAsync(eventId);

        if (ev is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        if (!ev.HasParticipant(currentUserId))
        {
            throw new UnauthorizedAccessException("Only a participant can delete the event.");
        }

        await _eventRepository.DeleteAsync(eventId);
    }
}