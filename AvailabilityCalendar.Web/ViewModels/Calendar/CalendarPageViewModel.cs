using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// View model for rendering the calendar page.
/// </summary>
public class CalendarPageViewModel
{
    /// <summary>
    /// Currently selected date.
    /// </summary>
    public DateTime CurrentDate { get; set; }

    /// <summary>
    /// Current calendar view type.
    /// </summary>
    public CalendarViewType ViewType { get; set; }

    /// <summary>
    /// Current calendar mode (personal or shared).
    /// </summary>
    public ViewMode Mode { get; set; }

    /// <summary>
    /// Minimum duration in minutes for free time intervals.
    /// </summary>
    public int MinimumDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Optional slot length in minutes for splitting free time.
    /// </summary>
    public int? SlotLengthMinutes { get; set; }

    /// <summary>
    /// Identifier of the current user.
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// Selected user identifiers for shared view.
    /// </summary>
    public List<Guid> SelectedUserIds { get; set; } = new();

    /// <summary>
    /// Users available for selection.
    /// </summary>
    public List<SelectableUserViewModel> AvailableUsers { get; set; } = new();

    /// <summary>
    /// Calendar grid cells for the current view.
    /// </summary>
    public List<CalendarCellViewModel> Cells { get; set; } = new();

    /// <summary>
    /// Events to display in personal mode.
    /// </summary>
    public List<EventDto> Events { get; set; } = new();

    /// <summary>
    /// Free intervals to display in shared mode.
    /// </summary>
    public List<TimeInterval> FreeIntervals { get; set; } = new();

    /// <summary>
    /// Visual blocks rendered in the calendar grid.
    /// </summary>
    public List<CalendarBlockViewModel> Blocks { get; set; } = new();

    /// <summary>
    /// Hour labels for the time axis.
    /// </summary>
    public List<int> HourLabels { get; set; } = new();

    /// <summary>
    /// Previous date for navigation.
    /// </summary>
    public DateTime PreviousDate { get; set; }

    /// <summary>
    /// Next date for navigation.
    /// </summary>
    public DateTime NextDate { get; set; }

    /// <summary>
    /// Start hour for the visible day range.
    /// </summary>
    public int DayStartHour { get; set; } = 0;

    /// <summary>
    /// End hour for the visible day range.
    /// </summary>
    public int DayEndHour { get; set; } = 24;

    /// <summary>
    /// Label describing the current mode.
    /// </summary>
    public string ModeLabel => Mode == ViewMode.Personal
        ? "Saját események"
        : "Közös szabad idősávok";

    /// <summary>
    /// Label describing the current period.
    /// </summary>
    public string CurrentPeriodLabel
    {
        get
        {
            return ViewType switch
            {
                CalendarViewType.Day => CurrentDate.ToString("yyyy.MM.dd."),
                CalendarViewType.Week => GetWeekLabel(),
                CalendarViewType.Month => CurrentDate.ToString("yyyy. MMMM"),
                _ => CurrentDate.ToString("yyyy.MM.dd.")
            };
        }
    }

    private string GetWeekLabel()
    {
        var startOfWeek = GetStartOfWeek(CurrentDate);
        var endOfWeek = startOfWeek.AddDays(6);

        return $"{startOfWeek:yyyy.MM.dd.} - {endOfWeek:yyyy.MM.dd.}";
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}