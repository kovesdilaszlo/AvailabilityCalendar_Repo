namespace AvailabilityCalendar.Application.DTOs;

/// <summary>
/// Data transfer object for a free time interval.
/// </summary>
public class FreeTimeIntervalDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public double DurationInMinutes { get; set; }
}