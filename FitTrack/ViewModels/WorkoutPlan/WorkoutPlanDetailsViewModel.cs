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
}
