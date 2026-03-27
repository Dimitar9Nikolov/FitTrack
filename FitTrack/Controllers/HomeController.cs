using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.Models;
using FitTrack.ViewModels;
using FitTrack.ViewModels.Home;
using FitTrack.ViewModels.Workout;

namespace FitTrack.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity!.IsAuthenticated)
            return View();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var user = await _context.Users.FindAsync(userId) as Models.ApplicationUser;

        var recentWorkouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
            .OrderByDescending(w => w.Date)
            .Take(5)
            .Select(w => new WorkoutListItemViewModel
            {
                Id = w.Id,
                Date = w.Date,
                DurationMinutes = w.DurationMinutes,
                Notes = w.Notes,
                WorkoutPlanTitle = w.WorkoutPlan != null ? w.WorkoutPlan.Title : null,
                ExerciseCount = w.WorkoutExercises.Count
            })
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            DisplayName = user?.DisplayName ?? User.Identity.Name ?? "User",
            TotalWorkouts = await _context.Workouts.CountAsync(w => w.UserId == userId),
            TotalDurationMinutes = await _context.Workouts
                .Where(w => w.UserId == userId)
                .SumAsync(w => (int?)w.DurationMinutes) ?? 0,
            TotalExercisesLogged = await _context.WorkoutExercises
                .CountAsync(we => we.Workout.UserId == userId),
            WorkoutsThisWeek = await _context.Workouts
                .CountAsync(w => w.UserId == userId && w.Date >= startOfWeek),
            WorkoutsThisMonth = await _context.Workouts
                .CountAsync(w => w.UserId == userId && w.Date >= startOfMonth),
            RecentWorkouts = recentWorkouts
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
