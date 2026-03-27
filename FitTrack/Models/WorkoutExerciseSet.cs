using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WorkoutExerciseSet
{
    public int Id { get; set; }

    public int WorkoutExerciseId { get; set; }

    [ForeignKey(nameof(WorkoutExerciseId))]
    public WorkoutExercise WorkoutExercise { get; set; } = null!;

    public int SetNumber { get; set; }

    [Range(1, 1000)]
    public int Reps { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal WeightKg { get; set; }
}
