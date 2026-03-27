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
    public List<DailyWorkoutSummary> WorkoutLog { get; set; } = new();
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

public class DailyWorkoutSummary
{
    public int WorkoutId { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? WorkoutPlanTitle { get; set; }
    public decimal TotalVolume { get; set; }             // sum of sets × reps × weight
    public int TotalCardioMinutes { get; set; }
    public List<WorkoutExerciseLogItem> Exercises { get; set; } = new();
}

public class WorkoutExerciseLogItem
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = "Strength";
    // Strength
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    // Cardio
    public int? CardioMinutes { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? Incline { get; set; }
}
