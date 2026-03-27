namespace FitTrack.ViewModels.WorkoutPlan;

public class WorkoutPlanIndexViewModel
{
    public List<WorkoutPlanListItemViewModel> Plans { get; set; } = new();
    public string? Search { get; set; }
    public string? Difficulty { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
}
