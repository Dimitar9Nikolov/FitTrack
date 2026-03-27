using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.Workout;

public class WorkoutExerciseFormViewModel
{
    public int WorkoutId { get; set; }

    [Required(ErrorMessage = "Please select an exercise.")]
    [Display(Name = "Exercise")]
    public int ExerciseId { get; set; }

    [Range(1, 100, ErrorMessage = "Sets must be between 1 and 100.")]
    public int Sets { get; set; } = 3;

    [Range(1, 1000, ErrorMessage = "Reps must be between 1 and 1000.")]
    public int Reps { get; set; } = 10;

    [Range(0, 1000, ErrorMessage = "Weight must be between 0 and 1000 kg.")]
    [Display(Name = "Weight (kg)")]
    public decimal WeightKg { get; set; } = 0;

    public IEnumerable<SelectListItem> Exercises { get; set; } = new List<SelectListItem>();
}
