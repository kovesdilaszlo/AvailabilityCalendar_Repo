using System.ComponentModel.DataAnnotations;

namespace AvailabilityCalendar.Web.ViewModels.Calendar;

/// <summary>
/// View model used when updating an existing event.
/// </summary>
public class UpdateEventViewModel
{
    /// <summary>
    /// Identifier of the event to update.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Updated title of the event.
    /// </summary>
    [Required(ErrorMessage = "A cím megadása kötelező.")]
    [Display(Name = "Cím")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Updated start date and time.
    /// </summary>
    [Required(ErrorMessage = "A kezdési időpont megadása kötelező.")]
    [Display(Name = "Kezdés")]
    [DataType(DataType.DateTime)]
    public DateTime Start { get; set; }

    /// <summary>
    /// Updated end date and time.
    /// </summary>
    [Required(ErrorMessage = "A befejezési időpont megadása kötelező.")]
    [Display(Name = "Befejezés")]
    [DataType(DataType.DateTime)]
    public DateTime End { get; set; }
}