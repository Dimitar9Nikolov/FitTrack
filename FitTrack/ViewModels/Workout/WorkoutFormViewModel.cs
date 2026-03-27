using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.Workout;

public class WorkoutFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Duration is required.")]
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; } = 60;

    [Display(Name = "Workout Plan (optional)")]
    public int? WorkoutPlanId { get; set; }

    public IEnumerable<SelectListItem> WorkoutPlans { get; set; } = new List<SelectListItem>();
}
