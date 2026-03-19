using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines availability calculations for users and time ranges.
/// </summary>
public interface IAvailabilityService
{
    ViewMode DetermineViewMode(List<Guid> selectedUsers, Guid currentUserId);
    List<Guid> NormalizeSelection(List<Guid> selectedUsers, Guid currentUserId);
    List<TimeInterval> MergeIntervals(List<TimeInterval> intervals);

    Task<List<TimeInterval>> GetCommonFreeTimeAsync(
        List<Guid> userIds,
        TimeInterval range,
        TimeSpan minimumDuration);
}