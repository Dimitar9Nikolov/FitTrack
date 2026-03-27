namespace FitTrack.ViewModels.Workout;

public class WorkoutExerciseViewModel
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = "Strength";

    // Strength — per-set breakdown
    public List<WorkoutSetViewModel> Sets { get; set; } = new();

    // Cardio
    public int? CardioMinutes { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? Incline { get; set; }
}
