namespace AvailabilityCalendar.Web.ViewModels.Calendar;

public class CalendarBlockViewModel
{
    public Guid? EventId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime Date { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsFreeTime { get; set; }

    public double TopPercent { get; set; }

    public double HeightPercent { get; set; }

    public int ColumnIndex { get; set; }

    public string TimeLabel
    {
        get
        {
            var startText = Start.ToString("HH:mm");

            var endText = End.Date > Start.Date && End.TimeOfDay == TimeSpan.Zero
                ? "24:00"
                : End.ToString("HH:mm");

            return $"{startText} - {endText}";
        }
    }
}