using Xunit;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.ValueObjects;
using Moq;

namespace AvailabilityCalendar.Test;

/// <summary>
/// Tests interval merging behavior in AvailabilityService.
/// </summary>
public class IntervalMergeServiceTests
{
    /// <summary>
    /// Verifies merging returns an empty list for empty input.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>();

        var result = service.MergeIntervals(intervals);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies a single interval is returned unchanged.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldReturnSameSingleInterval_WhenInputContainsOneInterval()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var interval = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var result = service.MergeIntervals(new List<TimeInterval> { interval });

        Assert.Single(result);
        Assert.Equal(interval.Start, result[0].Start);
        Assert.Equal(interval.End, result[0].End);
    }

    /// <summary>
    /// Verifies overlapping intervals are merged into one.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldMergeOverlappingIntervals()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 11, 0, 0), new DateTime(2026, 1, 1, 13, 0, 0))
        };

        var result = service.MergeIntervals(intervals);

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), result[0].End);
    }

    /// <summary>
    /// Verifies chained overlaps are merged into a single interval.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldMergeMultipleChainedOverlaps()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 11, 30, 0), new DateTime(2026, 1, 1, 13, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 45, 0), new DateTime(2026, 1, 1, 14, 0, 0))
        };

        var result = service.MergeIntervals(intervals);

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].End);
    }

    /// <summary>
    /// Verifies non-overlapping intervals remain separate.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldKeepSeparateIntervals_WhenTheyDoNotOverlap()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 11, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 0, 0), new DateTime(2026, 1, 1, 13, 0, 0))
        };

        var result = service.MergeIntervals(intervals);

        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies touching intervals are merged into one.
    /// </summary>
    [Fact]
    public void MergeIntervals_ShouldMergeTouchingIntervals()
    {
        var service = new AvailabilityService(new Mock<IEventRepository>().Object);
        var intervals = new List<TimeInterval>
        {
            new(new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0)),
            new(new DateTime(2026, 1, 1, 12, 0, 0), new DateTime(2026, 1, 1, 14, 0, 0))
        };

        var result = service.MergeIntervals(intervals);

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2026, 1, 1, 14, 0, 0), result[0].End);
    }
}