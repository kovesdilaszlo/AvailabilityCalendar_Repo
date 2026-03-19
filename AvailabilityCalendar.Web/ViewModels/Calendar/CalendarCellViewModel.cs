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
    /// Indicates whether the cell belongs to the current period.
    /// </summary>
    public bool IsCurrentPeriod { get; set; }

    /// <summary>
    /// Indicates whether the cell represents today.
    /// </summary>
    public bool IsToday { get; set; }

    /// <summary>
    /// Indicates whether the cell is currently selected.
    /// </summary>
    public bool IsSelectedDate { get; set; }
}