namespace AvailabilityCalendar.Application.Commands;

/// <summary>
/// Command payload for updating an event.
/// </summary>
public class UpdateEventCommand
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid CurrentUserId { get; set; }
}