namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// Represents a single calendar cell in the UI.
/// </summary>
public class CalendarCellViewModel
{
    /// <summary>
    /// Date represented by the calendar cell.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Indicates whether the cell belongs to the currently displayed period.
    /// </summary>
    public bool IsCurrentPeriod { get; set; }

    /// <summary>
    /// Indicates whether the cell represents today's date.
    /// </summary>
    public bool IsToday { get; set; }

    /// <summary>
    /// Indicates whether the cell is the actively selected date.
    /// </summary>
    public bool IsSelectedDate { get; set; }
}