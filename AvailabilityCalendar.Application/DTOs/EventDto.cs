namespace AvailabilityCalendar.Application.DTOs;

/// <summary>
/// Data transfer object representing an event and its participants.
/// </summary>
/// <remarks>
/// The system no longer stores a dedicated "creator" for events.
/// Therefore, only participant-based data is exposed.
/// </remarks>
public class EventDto
{
    /// <summary>
    /// Unique identifier of the event.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the event.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Start date and time of the event.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// End date and time of the event.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Identifiers of users participating in the event.
    /// </summary>
    public List<Guid> ParticipantIds { get; set; } = new();
}