using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Services;

/// <summary>
/// Implements event management operations.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> CreateEventAsync(CreateEventCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ArgumentException("Event title cannot be empty.");
        }

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            CreatedByUserId = command.CurrentUserId
        };

        ev.UpdateTime(command.Start, command.End);

        ev.AddParticipant(command.CurrentUserId);

        foreach (var participantId in command.ParticipantIds.Distinct())
        {
            ev.AddParticipant(participantId);
        }

        await _eventRepository.AddAsync(ev);

        return ev.Id;
    }

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
                CreatedByUserId = e.CreatedByUserId,
                ParticipantIds = e.Participants
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList()
            })
            .ToList();
    }

    public async Task UpdateEventAsync(UpdateEventCommand command)
    {
        var ev = await _eventRepository.GetByIdAsync(command.EventId);

        if (ev is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        if (!ev.IsCreatedBy(command.CurrentUserId))
        {
            throw new UnauthorizedAccessException("Only the creator can update the event.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new ArgumentException("Event title cannot be empty.");
        }

        ev.Title = command.Title.Trim();
        ev.UpdateTime(command.Start, command.End);

        await _eventRepository.UpdateAsync(ev);
    }

    public async Task DeleteEventAsync(Guid eventId, Guid currentUserId)
    {
        var ev = await _eventRepository.GetByIdAsync(eventId);

        if (ev is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        if (!ev.IsCreatedBy(currentUserId))
        {
            throw new UnauthorizedAccessException("Only the creator can delete the event.");
        }

        await _eventRepository.DeleteAsync(eventId);
    }
}