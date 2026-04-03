namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// Represents a visual block in the calendar UI.
/// A block can display either a personal event or a free interval.
/// </summary>
public class CalendarBlockViewModel
{
    /// <summary>
    /// Identifier of the related event.
    /// Null when the block represents free time.
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Date represented by the block.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Start time of the block.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// End time of the block.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Display title of the block.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the block represents free time.
    /// </summary>
    public bool IsFreeTime { get; set; }

    /// <summary>
    /// Vertical position of the block in the day column, expressed as a percentage.
    /// </summary>
    public double TopPercent { get; set; }

    /// <summary>
    /// Height of the block in the day column, expressed as a percentage.
    /// </summary>
    public double HeightPercent { get; set; }

    /// <summary>
    /// Column index used in multi-day views.
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Human-readable label showing the start and end time.
    /// </summary>
    public string TimeLabel
    {
        get
        {
            var startText = Start.ToString("HH:mm");

            var endText = End.Date > Start.Date && End.TimeOfDay == TimeSpan.Zero
                ? "24:00"
                : End.ToString("HH:mm");

            return $"{startText} - {endText}";
        }
    }
}