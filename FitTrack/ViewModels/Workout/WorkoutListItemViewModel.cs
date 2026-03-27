namespace FitTrack.ViewModels.Workout;

public class WorkoutListItemViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public string? WorkoutPlanTitle { get; set; }
    public int ExerciseCount { get; set; }
}
