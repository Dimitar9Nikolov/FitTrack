using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WorkoutPlanExercise
{
    public int Id { get; set; }

    public int WorkoutPlanId { get; set; }

    [ForeignKey(nameof(WorkoutPlanId))]
    public WorkoutPlan WorkoutPlan { get; set; } = null!;

    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise Exercise { get; set; } = null!;

    public int Sets { get; set; }
    public int Reps { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public int OrderIndex { get; set; }
}
