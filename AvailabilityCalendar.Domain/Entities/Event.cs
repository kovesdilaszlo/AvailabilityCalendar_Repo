namespace AvailabilityCalendar.Domain.Entities;

/// <summary>
/// Represents a scheduled event with a time range and participants.
/// </summary>
public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();

    public void AddParticipant(Guid userId)
    {
        if (HasParticipant(userId))
        {
            return;
        }

        Participants.Add(new EventParticipant
        {
            EventId = Id,
            UserId = userId
        });
    }

    public void RemoveParticipant(Guid userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);

        if (participant is null)
        {
            return;
        }

        Participants.Remove(participant);
    }

    public bool HasParticipant(Guid userId)
    {
        return Participants.Any(p => p.UserId == userId);
    }

    public void UpdateTime(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }

        Start = start;
        End = end;
    }
}