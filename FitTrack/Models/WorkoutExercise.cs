using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WorkoutExercise
{
    public int Id { get; set; }

    public int WorkoutId { get; set; }

    [ForeignKey(nameof(WorkoutId))]
    public Workout Workout { get; set; } = null!;

    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise Exercise { get; set; } = null!;

    // ── Cardio fields ──────────────────────────────────────────────────────────
    public int? CardioMinutes { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? Incline { get; set; }

    // ── Strength sets (one row per set) ────────────────────────────────────────
    public ICollection<WorkoutExerciseSet> ExerciseSets { get; set; } = new List<WorkoutExerciseSet>();
}
