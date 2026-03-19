using System.ComponentModel.DataAnnotations;

namespace AvailabilityCalendar.Web.ViewModels.Calendar;

public class UpdateEventViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "A cím megadása kötelező.")]
    [Display(Name = "Cím")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A kezdési időpont megadása kötelező.")]
    [Display(Name = "Kezdés")]
    [DataType(DataType.DateTime)]
    public DateTime Start { get; set; }

    [Required(ErrorMessage = "A befejezési időpont megadása kötelező.")]
    [Display(Name = "Befejezés")]
    [DataType(DataType.DateTime)]
    public DateTime End { get; set; }
}