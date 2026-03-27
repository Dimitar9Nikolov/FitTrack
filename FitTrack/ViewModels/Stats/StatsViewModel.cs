namespace FitTrack.ViewModels.Stats;

public class StatsViewModel
{
    public int TotalWorkouts { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int TotalExercisesLogged { get; set; }
    public int WorkoutsThisWeek { get; set; }
    public int WorkoutsThisMonth { get; set; }

    public string? FavoriteMuscleGroup { get; set; }
    public string? FavoriteExercise { get; set; }

    public List<MonthlyWorkoutData> MonthlyData { get; set; } = new();
    public List<ExerciseVolumeData> TopExercises { get; set; } = new();
}

public class MonthlyWorkoutData
{
    public string Month { get; set; } = string.Empty;   // "Jan 2026"
    public int WorkoutCount { get; set; }
    public int TotalMinutes { get; set; }
}

public class ExerciseVolumeData
{
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public int TimesLogged { get; set; }
    public decimal TotalVolume { get; set; }             // sets × reps × weight
}
