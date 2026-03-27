using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WorkoutPlan
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = string.Empty;

    [Required]
    public string CreatorId { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatorId))]
    public ApplicationUser Creator { get; set; } = null!;

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<WorkoutPlanExercise> PlanExercises { get; set; } = new List<WorkoutPlanExercise>();
}
