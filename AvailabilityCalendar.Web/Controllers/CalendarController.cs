using AvailabilityCalendar.Application.Commands;
using AvailabilityCalendar.Application.DTOs;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;
using AvailabilityCalendar.Web.Extensions;
using AvailabilityCalendar.Web.ViewModels.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityCalendar.Web.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly IEventService _eventService;
    private readonly IAvailabilityService _availabilityService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the calendar controller.
    /// </summary>
    public CalendarController(
        IEventService eventService,
        IAvailabilityService availabilityService,
        IUserRepository userRepository)
    {
        _eventService = eventService;
        _availabilityService = availabilityService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Displays the calendar for the requested date and view.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? date,
        CalendarViewType view = CalendarViewType.Day,
        List<Guid>? selectedUsers = null,
        int minimumDurationMinutes = 60,
        int? slotLengthMinutes = null)
    {
        var currentUserId = User.GetUserId();
        var currentDate = (date ?? DateTime.Today).Date;

        if (minimumDurationMinutes < 1)
        {
            minimumDurationMinutes = 1;
        }

        if (slotLengthMinutes.HasValue && slotLengthMinutes.Value < 1)
        {
            slotLengthMinutes = null;
        }

        if (slotLengthMinutes.HasValue && slotLengthMinutes.Value < minimumDurationMinutes)
        {
            slotLengthMinutes = minimumDurationMinutes;
        }

        var normalizedSelection = _availabilityService.NormalizeSelection(
            selectedUsers ?? new List<Guid>(),
            currentUserId);

        var mode = _availabilityService.DetermineViewMode(normalizedSelection, currentUserId);
        var range = BuildRange(view, currentDate);
        var allUsers = await _userRepository.GetAllAsync();

        var model = new CalendarPageViewModel
        {
            CurrentDate = currentDate,
            CurrentUserId = currentUserId,
            ViewType = view,
            Mode = mode,
            MinimumDurationMinutes = minimumDurationMinutes,
            SlotLengthMinutes = slotLengthMinutes,
            SelectedUserIds = normalizedSelection,
            AvailableUsers = allUsers
                .Where(u => u.Id != currentUserId)
                .Select(u => new SelectableUserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    IsSelected = normalizedSelection.Contains(u.Id),
                    IsCurrentUser = false
                })
                .ToList(),
            Cells = BuildCells(view, currentDate),
            PreviousDate = GetPreviousDate(view, currentDate),
            NextDate = GetNextDate(view, currentDate),
            HourLabels = Enumerable.Range(0, 25).ToList(),
            DayStartHour = 0,
            DayEndHour = 24
        };

        if (mode == ViewMode.Personal)
        {
            model.Events = await _eventService.GetEventsByUserAsync(currentUserId, range);
        }
        else
        {
            var freeIntervals = await _availabilityService.GetCommonFreeTimeAsync(
                normalizedSelection,
                range,
                TimeSpan.FromMinutes(minimumDurationMinutes));

            if (slotLengthMinutes.HasValue)
            {
                freeIntervals = SplitIntervalsIntoSlots(
                    freeIntervals,
                    TimeSpan.FromMinutes(slotLengthMinutes.Value),
                    TimeSpan.FromMinutes(minimumDurationMinutes));
            }

            model.FreeIntervals = freeIntervals;
        }

        model.Blocks = BuildBlocks(
            view,
            model.Mode,
            model.Events,
            model.FreeIntervals,
            range,
            model.DayStartHour,
            model.DayEndHour);

        return View(model);
    }

    /// <summary>
    /// Shows the event creation form.
    /// </summary>
    [HttpGet]
    public IActionResult Create(DateTime? date)
    {
        var start = (date ?? DateTime.Today).Date.AddHours(9);
        var end = start.AddHours(1);

        var model = new CreateEventViewModel
        {
            Start = start,
            End = end
        };

        return View(model);
    }

    /// <summary>
    /// Creates a new event from form input.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventViewModel model)
    {
        if (model.End <= model.Start)
        {
            ModelState.AddModelError(nameof(model.End), "A befejezési időnek későbbinek kell lennie, mint a kezdési idő.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateEventCommand
        {
            Title = model.Title.Trim(),
            Start = model.Start,
            End = model.End,
            CurrentUserId = User.GetUserId(),
            ParticipantIds = new List<Guid>()
        };

        await _eventService.CreateEventAsync(command);

        return RedirectToAction(nameof(Index), new
        {
            date = model.Start.Date,
            view = CalendarViewType.Day
        });
    }

    /// <summary>
    /// Creates a new event based on a selected free time slot.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromFreeSlot(
        string title,
        DateTime start,
        DateTime end,
        List<Guid> participantIds,
        DateTime? redirectDate = null,
        CalendarViewType redirectView = CalendarViewType.Day,
        int minimumDurationMinutes = 60,
        int? slotLengthMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Közös esemény";
        }

        if (end <= start)
        {
            return RedirectToAction(nameof(Index), new
            {
                date = (redirectDate ?? DateTime.Today).Date,
                view = redirectView,
                selectedUsers = participantIds,
                minimumDurationMinutes,
                slotLengthMinutes
            });
        }

        var command = new CreateEventCommand
        {
            Title = title.Trim(),
            Start = start,
            End = end,
            CurrentUserId = User.GetUserId(),
            ParticipantIds = participantIds ?? new List<Guid>()
        };

        await _eventService.CreateEventAsync(command);

        return RedirectToAction(nameof(Index), new
        {
            date = (redirectDate ?? start.Date).Date,
            view = redirectView,
            selectedUsers = participantIds,
            minimumDurationMinutes,
            slotLengthMinutes
        });
    }

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        UpdateEventViewModel model,
        DateTime? redirectDate = null,
        CalendarViewType redirectView = CalendarViewType.Day,
        int minimumDurationMinutes = 60,
        int? slotLengthMinutes = null,
        List<Guid>? selectedUsers = null)
    {
        if (model.End <= model.Start)
        {
            ModelState.AddModelError(nameof(model.End), "A befejezési időnek későbbinek kell lennie, mint a kezdési idő.");
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index), new
            {
                date = (redirectDate ?? model.Start.Date).Date,
                view = redirectView,
                selectedUsers,
                minimumDurationMinutes,
                slotLengthMinutes
            });
        }

        var command = new UpdateEventCommand
        {
            EventId = model.Id,
            Title = model.Title.Trim(),
            Start = model.Start,
            End = model.End,
            CurrentUserId = User.GetUserId()
        };

        await _eventService.UpdateEventAsync(command);

        return RedirectToAction(nameof(Index), new
        {
            date = (redirectDate ?? model.Start.Date).Date,
            view = redirectView,
            selectedUsers,
            minimumDurationMinutes,
            slotLengthMinutes
        });
    }

    /// <summary>
    /// Deletes an event.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        Guid id,
        DateTime? redirectDate = null,
        CalendarViewType redirectView = CalendarViewType.Day,
        int minimumDurationMinutes = 60,
        int? slotLengthMinutes = null,
        List<Guid>? selectedUsers = null)
    {
        await _eventService.DeleteEventAsync(id, User.GetUserId());

        return RedirectToAction(nameof(Index), new
        {
            date = (redirectDate ?? DateTime.Today).Date,
            view = redirectView,
            selectedUsers,
            minimumDurationMinutes,
            slotLengthMinutes
        });
    }

    /// <summary>
    /// Builds the visible date range for the selected view.
    /// </summary>
    private static TimeInterval BuildRange(CalendarViewType view, DateTime currentDate)
    {
        return view switch
        {
            CalendarViewType.Day => new TimeInterval(currentDate.Date, currentDate.Date.AddDays(1)),
            CalendarViewType.Week => new TimeInterval(
                GetStartOfWeek(currentDate),
                GetStartOfWeek(currentDate).AddDays(7)),
            CalendarViewType.Month => new TimeInterval(
                new DateTime(currentDate.Year, currentDate.Month, 1),
                new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1)),
            _ => new TimeInterval(currentDate.Date, currentDate.Date.AddDays(1))
        };
    }

    /// <summary>
    /// Builds the calendar grid cells for the selected view.
    /// </summary>
    private static List<CalendarCellViewModel> BuildCells(CalendarViewType view, DateTime currentDate)
    {
        return view switch
        {
            CalendarViewType.Day => BuildDayCells(currentDate),
            CalendarViewType.Week => BuildWeekCells(currentDate),
            CalendarViewType.Month => BuildMonthCells(currentDate),
            _ => BuildDayCells(currentDate)
        };
    }

    /// <summary>
    /// Builds the single day cell for day view.
    /// </summary>
    private static List<CalendarCellViewModel> BuildDayCells(DateTime currentDate)
    {
        return new List<CalendarCellViewModel>
        {
            new CalendarCellViewModel
            {
                Date = currentDate.Date,
                IsCurrentPeriod = true,
                IsToday = currentDate.Date == DateTime.Today,
                IsSelectedDate = true
            }
        };
    }

    /// <summary>
    /// Builds the grid cells for week view.
    /// </summary>
    private static List<CalendarCellViewModel> BuildWeekCells(DateTime currentDate)
    {
        var startOfWeek = GetStartOfWeek(currentDate);

        return Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = startOfWeek.AddDays(offset);

                return new CalendarCellViewModel
                {
                    Date = date,
                    IsCurrentPeriod = true,
                    IsToday = date.Date == DateTime.Today,
                    IsSelectedDate = date.Date == currentDate.Date
                };
            })
            .ToList();
    }

    /// <summary>
    /// Builds the grid cells for month view.
    /// </summary>
    private static List<CalendarCellViewModel> BuildMonthCells(DateTime currentDate)
    {
        var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        var gridStart = GetStartOfWeek(firstDayOfMonth);

        return Enumerable.Range(0, 42)
            .Select(offset =>
            {
                var date = gridStart.AddDays(offset);

                return new CalendarCellViewModel
                {
                    Date = date,
                    IsCurrentPeriod = date.Month == currentDate.Month,
                    IsToday = date.Date == DateTime.Today,
                    IsSelectedDate = date.Date == currentDate.Date
                };
            })
            .ToList();
    }

    /// <summary>
    /// Gets the previous date for the selected view.
    /// </summary>
    private static DateTime GetPreviousDate(CalendarViewType view, DateTime currentDate)
    {
        return view switch
        {
            CalendarViewType.Day => currentDate.AddDays(-1),
            CalendarViewType.Week => currentDate.AddDays(-7),
            CalendarViewType.Month => currentDate.AddMonths(-1),
            _ => currentDate.AddDays(-1)
        };
    }

    /// <summary>
    /// Gets the next date for the selected view.
    /// </summary>
    private static DateTime GetNextDate(CalendarViewType view, DateTime currentDate)
    {
        return view switch
        {
            CalendarViewType.Day => currentDate.AddDays(1),
            CalendarViewType.Week => currentDate.AddDays(7),
            CalendarViewType.Month => currentDate.AddMonths(1),
            _ => currentDate.AddDays(1)
        };
    }

    /// <summary>
    /// Splits free time intervals into slot-sized segments.
    /// </summary>
    private static List<TimeInterval> SplitIntervalsIntoSlots(
        IEnumerable<TimeInterval> intervals,
        TimeSpan slotLength,
        TimeSpan minimumDuration)
    {
        if (slotLength <= TimeSpan.Zero)
        {
            return intervals.ToList();
        }

        var result = new List<TimeInterval>();

        foreach (var interval in intervals)
        {
            var currentStart = interval.Start;

            while (currentStart < interval.End)
            {
                var currentEnd = currentStart.Add(slotLength);

                if (currentEnd > interval.End)
                {
                    currentEnd = interval.End;
                }

                if (currentEnd - currentStart >= minimumDuration)
                {
                    result.Add(new TimeInterval(currentStart, currentEnd));
                }

                currentStart = currentEnd;
            }
        }

        return result;
    }

    /// <summary>
    /// Builds visual blocks for events or free time depending on view mode.
    /// </summary>
    private static List<CalendarBlockViewModel> BuildBlocks(
        CalendarViewType view,
        ViewMode mode,
        List<EventDto> events,
        List<TimeInterval> freeIntervals,
        TimeInterval visibleRange,
        int dayStartHour,
        int dayEndHour)
    {
        if (view == CalendarViewType.Month)
        {
            return new List<CalendarBlockViewModel>();
        }

        var blocks = new List<CalendarBlockViewModel>();

        if (mode == ViewMode.Personal)
        {
            foreach (var ev in events)
            {
                blocks.AddRange(BuildEventBlocks(ev, view, visibleRange, dayStartHour, dayEndHour));
            }
        }
        else
        {
            foreach (var interval in freeIntervals)
            {
                blocks.AddRange(BuildFreeTimeBlocks(interval, view, visibleRange, dayStartHour, dayEndHour));
            }
        }

        return blocks
            .OrderBy(b => b.Date)
            .ThenBy(b => b.Start)
            .ToList();
    }

    /// <summary>
    /// Builds display blocks for a single event.
    /// </summary>
    private static IEnumerable<CalendarBlockViewModel> BuildEventBlocks(
        EventDto ev,
        CalendarViewType view,
        TimeInterval visibleRange,
        int dayStartHour,
        int dayEndHour)
    {
        return SplitIntoDailyBlocks(
            ev.Id,
            ev.CreatedByUserId,
            ev.Start,
            ev.End,
            ev.Title,
            isFreeTime: false,
            view,
            visibleRange,
            dayStartHour,
            dayEndHour);
    }

    /// <summary>
    /// Builds display blocks for a free time interval.
    /// </summary>
    private static IEnumerable<CalendarBlockViewModel> BuildFreeTimeBlocks(
        TimeInterval interval,
        CalendarViewType view,
        TimeInterval visibleRange,
        int dayStartHour,
        int dayEndHour)
    {
        return SplitIntoDailyBlocks(
            null,
            null,
            interval.Start,
            interval.End,
            "Szabad",
            isFreeTime: true,
            view,
            visibleRange,
            dayStartHour,
            dayEndHour);
    }

    /// <summary>
    /// Splits a time span into day-aligned blocks for display.
    /// </summary>
    private static IEnumerable<CalendarBlockViewModel> SplitIntoDailyBlocks(
        Guid? eventId,
        Guid? createdByUserId,
        DateTime start,
        DateTime end,
        string title,
        bool isFreeTime,
        CalendarViewType view,
        TimeInterval visibleRange,
        int dayStartHour,
        int dayEndHour)
    {
        var result = new List<CalendarBlockViewModel>();

        var clippedStart = start < visibleRange.Start ? visibleRange.Start : start;
        var clippedEnd = end > visibleRange.End ? visibleRange.End : end;

        if (clippedEnd <= clippedStart)
        {
            return result;
        }

        var currentDay = clippedStart.Date;
        var lastDay = clippedEnd.Date;

        while (currentDay <= lastDay)
        {
            var segmentStart = clippedStart > currentDay
                ? clippedStart
                : currentDay;

            var segmentEnd = clippedEnd < currentDay.AddDays(1)
                ? clippedEnd
                : currentDay.AddDays(1);

            var visibleSegment = ClipToVisibleHours(segmentStart, segmentEnd, dayStartHour, dayEndHour);

            if (visibleSegment is not null)
            {
                var (visibleStart, visibleEnd) = visibleSegment.Value;

                var block = BuildBlock(
                    eventId,
                    createdByUserId,
                    currentDay,
                    visibleStart,
                    visibleEnd,
                    title,
                    isFreeTime,
                    view,
                    visibleRange,
                    dayStartHour,
                    dayEndHour);

                if (block is not null)
                {
                    result.Add(block);
                }
            }

            currentDay = currentDay.AddDays(1);
        }

        return result;
    }

    /// <summary>
    /// Builds a single display block for a day segment.
    /// </summary>
    private static CalendarBlockViewModel? BuildBlock(
        Guid? eventId,
        Guid? createdByUserId,
        DateTime day,
        DateTime start,
        DateTime end,
        string title,
        bool isFreeTime,
        CalendarViewType view,
        TimeInterval visibleRange,
        int dayStartHour,
        int dayEndHour)
    {
        if (end <= start)
        {
            return null;
        }

        var totalMinutes = (dayEndHour - dayStartHour) * 60.0;
        if (totalMinutes <= 0)
        {
            return null;
        }

        var dayStart = day.Date.AddHours(dayStartHour);
        var minutesFromDayStart = (start - dayStart).TotalMinutes;
        var durationMinutes = (end - start).TotalMinutes;

        if (durationMinutes <= 0)
        {
            return null;
        }

        var columnIndex = view switch
        {
            CalendarViewType.Day => 0,
            CalendarViewType.Week => (int)(day.Date - GetStartOfWeek(visibleRange.Start)).TotalDays,
            _ => 0
        };

        return new CalendarBlockViewModel
        {
            EventId = eventId,
            CreatedByUserId = createdByUserId,
            Date = day.Date,
            Start = start,
            End = end,
            Title = title,
            IsFreeTime = isFreeTime,
            TopPercent = (minutesFromDayStart / totalMinutes) * 100.0,
            HeightPercent = (durationMinutes / totalMinutes) * 100.0,
            ColumnIndex = columnIndex
        };
    }

    /// <summary>
    /// Clips a time range to the visible hours of a day.
    /// </summary>
    private static (DateTime Start, DateTime End)? ClipToVisibleHours(
        DateTime start,
        DateTime end,
        int dayStartHour,
        int dayEndHour)
    {
        var visibleStart = start.Date.AddHours(dayStartHour);
        var visibleEnd = start.Date.AddHours(dayEndHour);

        var clippedStart = start < visibleStart ? visibleStart : start;
        var clippedEnd = end > visibleEnd ? visibleEnd : end;

        if (clippedEnd <= clippedStart)
        {
            return null;
        }

        return (clippedStart, clippedEnd);
    }

    /// <summary>
    /// Calculates the Monday start of the week for the given date.
    /// </summary>
    private static DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}