namespace AvailabilityCalendar.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Name cannot be empty.");
        }

        Name = newName;
    }
}