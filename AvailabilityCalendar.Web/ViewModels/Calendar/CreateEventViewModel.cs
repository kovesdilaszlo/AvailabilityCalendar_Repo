using System.ComponentModel.DataAnnotations;

namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// View model used when creating a new event from the UI.
/// </summary>
public class CreateEventViewModel
{
    /// <summary>
    /// Title of the event.
    /// </summary>
    [Required(ErrorMessage = "A cím megadása kötelező.")]
    [Display(Name = "Cím")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Start date and time of the event.
    /// </summary>
    [Required(ErrorMessage = "A kezdési időpont megadása kötelező.")]
    [Display(Name = "Kezdés")]
    [DataType(DataType.DateTime)]
    public DateTime Start { get; set; }

    /// <summary>
    /// End date and time of the event.
    /// </summary>
    [Required(ErrorMessage = "A befejezési időpont megadása kötelező.")]
    [Display(Name = "Befejezés")]
    [DataType(DataType.DateTime)]
    public DateTime End { get; set; }
}