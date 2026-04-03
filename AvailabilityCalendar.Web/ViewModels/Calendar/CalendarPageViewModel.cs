using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// View model used to render the main calendar page.
/// </summary>
public class CalendarPageViewModel
{
    /// <summary>
    /// Currently selected date.
    /// </summary>
    public DateTime CurrentDate { get; set; }

    /// <summary>
    /// Active calendar view type.
    /// </summary>
    public CalendarViewType ViewType { get; set; }

    /// <summary>
    /// Current calendar mode.
    /// </summary>
    public ViewMode Mode { get; set; }

    /// <summary>
    /// Minimum duration in minutes for shared free intervals.
    /// </summary>
    public int MinimumDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Optional slot length in minutes used for splitting free intervals.
    /// </summary>
    public int? SlotLengthMinutes { get; set; }

    /// <summary>
    /// Identifier of the currently signed-in user.
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// Selected user identifiers in shared mode.
    /// </summary>
    public List<Guid> SelectedUserIds { get; set; } = new();

    /// <summary>
    /// Users that can be selected for shared availability search.
    /// </summary>
    public List<SelectableUserViewModel> AvailableUsers { get; set; } = new();

    /// <summary>
    /// Calendar cells displayed in the current view.
    /// </summary>
    public List<CalendarCellViewModel> Cells { get; set; } = new();

    /// <summary>
    /// Events displayed in personal mode.
    /// </summary>
    public List<EventDto> Events { get; set; } = new();

    /// <summary>
    /// Free intervals displayed in shared mode.
    /// </summary>
    public List<TimeInterval> FreeIntervals { get; set; } = new();

    /// <summary>
    /// Visual blocks rendered into the calendar grid.
    /// </summary>
    public List<CalendarBlockViewModel> Blocks { get; set; } = new();

    /// <summary>
    /// Hour labels shown along the time axis.
    /// </summary>
    public List<int> HourLabels { get; set; } = new();

    /// <summary>
    /// Previous date used for navigation.
    /// </summary>
    public DateTime PreviousDate { get; set; }

    /// <summary>
    /// Next date used for navigation.
    /// </summary>
    public DateTime NextDate { get; set; }

    /// <summary>
    /// First visible hour of the day.
    /// </summary>
    public int DayStartHour { get; set; } = 0;

    /// <summary>
    /// Last visible hour of the day.
    /// </summary>
    public int DayEndHour { get; set; } = 24;

    /// <summary>
    /// Display label of the active mode.
    /// </summary>
    public string ModeLabel => Mode == ViewMode.Personal
        ? "Saját események"
        : "Közös szabad idősávok";

    /// <summary>
    /// Display label of the currently shown period.
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

    /// <summary>
    /// Builds the display label for the current week.
    /// </summary>
    private string GetWeekLabel()
    {
        var startOfWeek = GetStartOfWeek(CurrentDate);
        var endOfWeek = startOfWeek.AddDays(6);

        return $"{startOfWeek:yyyy.MM.dd.} - {endOfWeek:yyyy.MM.dd.}";
    }

    /// <summary>
    /// Calculates the Monday start of the week.
    /// </summary>
    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}