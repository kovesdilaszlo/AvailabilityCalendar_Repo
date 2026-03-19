namespace AvailabilityCalendar.Domain.Entities;

public class EventParticipant
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}