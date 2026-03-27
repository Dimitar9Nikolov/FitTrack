using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.Workout;

public class WorkoutExerciseFormViewModel
{
    public int WorkoutId { get; set; }

    [Required(ErrorMessage = "Please select an exercise.")]
    [Display(Name = "Exercise")]
    public int ExerciseId { get; set; }

    // ── Strength ───────────────────────────────────────────────────────────────
    [Range(1, 100)]
    public int Sets { get; set; } = 3;

    [Range(1, 1000)]
    public int Reps { get; set; } = 10;

    [Range(0, 1000)]
    [Display(Name = "Weight (kg)")]
    public decimal WeightKg { get; set; } = 0;

    // ── Cardio ─────────────────────────────────────────────────────────────────
    [Range(1, 600)]
    [Display(Name = "Duration (min)")]
    public int CardioMinutes { get; set; } = 30;

    [Range(0, 100)]
    [Display(Name = "Speed (km/h)")]
    public decimal SpeedKmh { get; set; } = 0;

    [Range(0, 30)]
    [Display(Name = "Incline (%)")]
    public decimal Incline { get; set; } = 0;

    // ── Dropdown data ──────────────────────────────────────────────────────────
    public IEnumerable<SelectListItem> Exercises { get; set; } = new List<SelectListItem>();

    /// <summary>ExerciseId → ExerciseType map for JS field toggling.</summary>
    public Dictionary<int, string> ExerciseTypeMap { get; set; } = new();
}
