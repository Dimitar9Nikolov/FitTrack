namespace FitTrack.ViewModels.Workout;

public class WorkoutDetailsViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public int? WorkoutPlanId { get; set; }
    public string? WorkoutPlanTitle { get; set; }

    public List<WorkoutExerciseViewModel> Exercises { get; set; } = new();
    public WorkoutExerciseFormViewModel AddExerciseForm { get; set; } = new();
}
