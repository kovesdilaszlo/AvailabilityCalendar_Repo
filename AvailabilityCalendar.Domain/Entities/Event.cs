namespace AvailabilityCalendar.Domain.Entities;

/// <summary>
/// Represents a scheduled event with a creator, time range, and participants.
/// </summary>
public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();

    /// <summary>
    /// Adds a participant if they are not already included.
    /// </summary>
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

    /// <summary>
    /// Removes a participant if they exist.
    /// </summary>
    public void RemoveParticipant(Guid userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);

        if (participant is null)
        {
            return;
        }

        Participants.Remove(participant);
    }

    /// <summary>
    /// Checks whether the participant is already included.
    /// </summary>
    public bool HasParticipant(Guid userId)
    {
        return Participants.Any(p => p.UserId == userId);
    }

    /// <summary>
    /// Updates start and end times, validating that end is not earlier than start.
    /// </summary>
    public void UpdateTime(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Determines whether the event was created by the specified user.
    /// </summary>
    public bool IsCreatedBy(Guid userId)
    {
        return CreatedByUserId == userId;
    }
}