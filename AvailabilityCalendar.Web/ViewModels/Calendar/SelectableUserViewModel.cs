namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// Represents a selectable user option in the calendar UI.
/// </summary>
public class SelectableUserViewModel
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the user is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Indicates whether the user is the current signed-in user.
    /// </summary>
    public bool IsCurrentUser { get; set; }
}