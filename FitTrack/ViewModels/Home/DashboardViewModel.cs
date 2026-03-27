using FitTrack.ViewModels.Workout;

namespace FitTrack.ViewModels.Home;

public class DashboardViewModel
{
    public string DisplayName { get; set; } = string.Empty;

    public int TotalWorkouts { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int TotalExercisesLogged { get; set; }
    public int WorkoutsThisWeek { get; set; }
    public int WorkoutsThisMonth { get; set; }

    public List<WorkoutListItemViewModel> RecentWorkouts { get; set; } = new();
}
