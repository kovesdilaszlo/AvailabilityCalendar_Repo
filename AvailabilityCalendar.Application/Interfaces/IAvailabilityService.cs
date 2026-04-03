using AvailabilityCalendar.Domain.Enums;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Application.Interfaces;

/// <summary>
/// Defines availability-related calculations used by the application.
/// </summary>
public interface IAvailabilityService
{
    /// <summary>
    /// Determines whether the current view should be personal or shared.
    /// </summary>
    ViewMode DetermineViewMode(List<Guid> selectedUsers, Guid currentUserId);

    /// <summary>
    /// Normalizes the selected users by removing duplicates
    /// and ensuring the current user is included.
    /// </summary>
    List<Guid> NormalizeSelection(List<Guid> selectedUsers, Guid currentUserId);

    /// <summary>
    /// Merges overlapping or touching time intervals into a normalized list.
    /// </summary>
    List<TimeInterval> MergeIntervals(List<TimeInterval> intervals);

    /// <summary>
    /// Calculates the common free time intervals for the given users
    /// within the specified time range.
    /// </summary>
    Task<List<TimeInterval>> GetCommonFreeTimeAsync(
        List<Guid> userIds,
        TimeInterval range,
        TimeSpan minimumDuration);
}