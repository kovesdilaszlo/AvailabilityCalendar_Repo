using Xunit;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Tests.DomainTests;

public class TimeIntervalEdgeCaseTests
{
    /// <summary>
    /// Verifies touching intervals are not considered overlapping.
    /// </summary>
    [Fact]
    public void OverlapsWith_ShouldReturnFalse_WhenIntervalsOnlyTouchAtBoundary()
    {
        // Arrange
        var first = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var second = new TimeInterval(
            new DateTime(2026, 1, 1, 12, 0, 0),
            new DateTime(2026, 1, 1, 14, 0, 0));

        // Act
        var result = first.OverlapsWith(second);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies an interval inside another is considered overlapping.
    /// </summary>
    [Fact]
    public void OverlapsWith_ShouldReturnTrue_WhenOneIntervalIsInsideAnother()
    {
        // Arrange
        var outer = new TimeInterval(
            new DateTime(2026, 1, 1, 9, 0, 0),
            new DateTime(2026, 1, 1, 17, 0, 0));

        var inner = new TimeInterval(
            new DateTime(2026, 1, 1, 11, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        // Act
        var result = outer.OverlapsWith(inner);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies merging identical intervals returns the same range.
    /// </summary>
    [Fact]
    public void MergeWith_ShouldReturnSameInterval_WhenIntervalsAreIdentical()
    {
        // Arrange
        var first = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var second = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        // Act
        var merged = first.MergeWith(second);

        // Assert
        Assert.Equal(first.Start, merged.Start);
        Assert.Equal(first.End, merged.End);
    }

    /// <summary>
    /// Verifies duration is zero when start equals end.
    /// </summary>
    [Fact]
    public void Duration_ShouldReturnZero_WhenStartEqualsEnd()
    {
        // Arrange
        var pointInterval = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 10, 0, 0));

        // Act
        var duration = pointInterval.Duration();

        // Assert
        Assert.Equal(TimeSpan.Zero, duration);
    }
}