using Microsoft.AspNetCore.Identity;

namespace FitTrack.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
