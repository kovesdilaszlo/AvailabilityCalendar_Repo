namespace AvailabilityCalendar.Application.Commands;

/// <summary>
/// Command payload for creating an event.
/// </summary>
public class CreateEventCommand
{
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid CurrentUserId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}