namespace AvailabilityCalendar.Domain.ValueObjects;

public class TimeInterval
{
    public DateTime Start { get; }
    public DateTime End { get; }

    /// <summary>
    /// Creates a time interval ensuring end is not earlier than start.
    /// </summary>
    public TimeInterval(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Determines whether this interval overlaps another.
    /// </summary>
    public bool OverlapsWith(TimeInterval other)
    {
        return Start < other.End && other.Start < End;
    }

    /// <summary>
    /// Merges another interval when overlapping or touching.
    /// </summary>
    public TimeInterval MergeWith(TimeInterval other)
    {
        if (!OverlapsWith(other) &&
            End != other.Start &&
            other.End != Start)
        {
            throw new InvalidOperationException("Cannot merge non-overlapping intervals.");
        }

        var mergedStart = Start < other.Start ? Start : other.Start;
        var mergedEnd = End > other.End ? End : other.End;

        return new TimeInterval(mergedStart, mergedEnd);
    }

    /// <summary>
    /// Gets the duration of the interval.
    /// </summary>
    public TimeSpan Duration()
    {
        return End - Start;
    }
}