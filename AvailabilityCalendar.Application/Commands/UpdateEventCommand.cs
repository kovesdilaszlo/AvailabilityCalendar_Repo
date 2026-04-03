namespace AvailabilityCalendar.Application.Commands;

/// <summary>
/// Command object used when updating an existing event.
/// </summary>
/// <remarks>
/// The current user identifier is required in order to validate
/// whether the user has permission to modify the event.
/// In the simplified model, this means checking if the user
/// is a participant of the event.
/// </remarks>
public class UpdateEventCommand
{
    /// <summary>
    /// Identifier of the event to update.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Updated title of the event.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Updated start date and time.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Updated end date and time.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Identifier of the currently authenticated user.
    /// Used for permission validation.
    /// </summary>
    public Guid CurrentUserId { get; set; }
}