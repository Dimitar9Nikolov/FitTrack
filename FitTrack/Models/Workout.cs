using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class Workout
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    public int? WorkoutPlanId { get; set; }

    [ForeignKey(nameof(WorkoutPlanId))]
    public WorkoutPlan? WorkoutPlan { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int DurationMinutes { get; set; }

    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
