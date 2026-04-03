namespace AvailabilityCalendar.Application.Commands;

/// <summary>
/// Command object used when creating a new event.
/// </summary>
/// <remarks>
/// The current user identifier is still needed, because the creator is
/// automatically added as a participant even though the system no longer
/// stores a separate creator field on the event itself.
/// </remarks>
public class CreateEventCommand
{
    /// <summary>
    /// Title of the new event.
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
    /// Identifier of the currently authenticated user.
    /// This user will always be added to the participant list.
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// Additional participants selected for the event.
    /// </summary>
    public List<Guid> ParticipantIds { get; set; } = new();
}