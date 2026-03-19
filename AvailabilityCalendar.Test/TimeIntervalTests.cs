using Xunit;
using AvailabilityCalendar.Domain.ValueObjects;

namespace AvailabilityCalendar.Tests.DomainTests;

/// <summary>
/// Tests TimeInterval core behaviors.
/// </summary>
public class TimeIntervalTests
{
    /// <summary>
    /// Verifies OverlapsWith returns true when intervals overlap.
    /// </summary>
    [Fact]
    public void OverlapsWith_ShouldReturnTrue_WhenIntervalsOverlap()
    {
        // Arrange
        var first = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var second = new TimeInterval(
            new DateTime(2026, 1, 1, 11, 0, 0),
            new DateTime(2026, 1, 1, 13, 0, 0));

        // Act
        var result = first.OverlapsWith(second);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies OverlapsWith returns false when intervals do not overlap.
    /// </summary>
    [Fact]
    public void OverlapsWith_ShouldReturnFalse_WhenIntervalsDoNotOverlap()
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
    /// Verifies MergeWith returns a combined interval for overlaps.
    /// </summary>
    [Fact]
    public void MergeWith_ShouldReturnMergedInterval_WhenIntervalsOverlap()
    {
        // Arrange
        var first = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var second = new TimeInterval(
            new DateTime(2026, 1, 1, 11, 0, 0),
            new DateTime(2026, 1, 1, 13, 0, 0));

        // Act
        var merged = first.MergeWith(second);

        // Assert
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), merged.Start);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0), merged.End);
    }

    /// <summary>
    /// Verifies Duration returns the expected time span.
    /// </summary>
    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var interval = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 30, 0));

        // Act
        var duration = interval.Duration();

        // Assert
        Assert.Equal(TimeSpan.FromHours(2.5), duration);
    }

    /// <summary>
    /// Verifies constructing a TimeInterval throws when end precedes start.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrowException_WhenEndIsEarlierThanStart()
    {
        // Arrange & Act
        var action = () => new TimeInterval(
            new DateTime(2026, 1, 1, 12, 0, 0),
            new DateTime(2026, 1, 1, 10, 0, 0));

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}