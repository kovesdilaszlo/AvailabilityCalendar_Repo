using AvailabilityCalendar.Domain.ValueObjects;
using Xunit;

namespace AvailabilityCalendar.Tests.DomainTests;

/// <summary>
/// Tests TimeInterval behavior.
/// </summary>
public class TimeIntervalTests
{
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

    [Fact]
    public void MergeWith_ShouldReturnSameResult_RegardlessOfOrder()
    {
        // Arrange
        var first = new TimeInterval(
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 1, 12, 0, 0));

        var second = new TimeInterval(
            new DateTime(2026, 1, 1, 11, 0, 0),
            new DateTime(2026, 1, 1, 13, 0, 0));

        // Act
        var merged1 = first.MergeWith(second);
        var merged2 = second.MergeWith(first);

        // Assert
        Assert.Equal(merged1.Start, merged2.Start);
        Assert.Equal(merged1.End, merged2.End);
    }

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