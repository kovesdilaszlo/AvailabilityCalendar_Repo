using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Services;

/// <summary>
/// Service responsible for calculating common free time intervals.
/// </summary>
public class AvailabilityService : IAvailabilityService
{
    private readonly IEventRepository _eventRepository;

    /// <summary>
    /// Creates a new availability service instance.
    /// </summary>
    public AvailabilityService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Determines whether the current calendar view is personal or shared.
    /// </summary>
    public ViewMode DetermineViewMode(List<Guid> selectedUsers, Guid currentUserId)
    {
        var normalized = NormalizeSelection(selectedUsers, currentUserId);
        return normalized.Count == 1 ? ViewMode.Personal : ViewMode.Shared;
    }

    /// <summary>
    /// Removes duplicates from the selected user list
    /// and makes sure the current user is always present.
    /// </summary>
    public List<Guid> NormalizeSelection(List<Guid> selectedUsers, Guid currentUserId)
    {
        selectedUsers ??= new List<Guid>();

        var result = selectedUsers
            .Distinct()
            .ToList();

        if (!result.Contains(currentUserId))
        {
            result.Add(currentUserId);
        }

        return result;
    }

    /// <summary>
    /// Merges overlapping or directly adjacent intervals into larger intervals.
    /// </summary>
    public List<TimeInterval> MergeIntervals(List<TimeInterval> intervals)
    {
        if (intervals is null || intervals.Count == 0)
        {
            return new List<TimeInterval>();
        }

        var ordered = intervals
            .OrderBy(i => i.Start)
            .ToList();

        var merged = new List<TimeInterval> { ordered[0] };

        for (int i = 1; i < ordered.Count; i++)
        {
            var current = ordered[i];
            var last = merged[^1];

            if (last.OverlapsWith(current) || last.End == current.Start)
            {
                merged[^1] = last.MergeWith(current);
            }
            else
            {
                merged.Add(current);
            }
        }

        return merged;
    }

    /// <summary>
    /// Calculates the common free time intervals for the selected users
    /// inside the specified visible time range.
    /// </summary>
    public async Task<List<TimeInterval>> GetCommonFreeTimeAsync(
        List<Guid> userIds,
        TimeInterval range,
        TimeSpan minimumDuration)
    {
        var normalizedUsers = userIds
            .Distinct()
            .ToList();

        var events = await _eventRepository.GetByUsersAsync(normalizedUsers, range);

        var busyIntervals = events
            .Select(e => new TimeInterval(e.Start, e.End))
            .Where(i => i.End > range.Start && i.Start < range.End)
            .Select(i =>
            {
                var start = i.Start < range.Start ? range.Start : i.Start;
                var end = i.End > range.End ? range.End : i.End;
                return new TimeInterval(start, end);
            })
            .ToList();

        var mergedBusy = MergeIntervals(busyIntervals);

        if (mergedBusy.Count == 0)
        {
            return range.Duration() >= minimumDuration
                ? new List<TimeInterval> { range }
                : new List<TimeInterval>();
        }

        var freeIntervals = new List<TimeInterval>();
        var cursor = range.Start;

        foreach (var busy in mergedBusy)
        {
            if (cursor < busy.Start)
            {
                freeIntervals.Add(new TimeInterval(cursor, busy.Start));
            }

            if (cursor < busy.End)
            {
                cursor = busy.End;
            }
        }

        if (cursor < range.End)
        {
            freeIntervals.Add(new TimeInterval(cursor, range.End));
        }

        return freeIntervals
            .Where(i => i.Duration() >= minimumDuration)
            .OrderBy(i => i.Start)
            .ToList();
    }
}