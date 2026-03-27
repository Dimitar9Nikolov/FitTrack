namespace FitTrack.ViewModels.WorkoutPlan;

public class WorkoutPlanDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public int WorkoutCount { get; set; }
    public bool IsOwner { get; set; }

    public List<PlanExerciseViewModel> PlanExercises { get; set; } = new();
}

public class PlanExerciseViewModel
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
}
