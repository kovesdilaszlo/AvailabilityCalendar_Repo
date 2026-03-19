namespace AvailabilityCalendar.Application.DTOs;

/// <summary>
/// Data transfer object for event details and participants.
/// </summary>
public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}