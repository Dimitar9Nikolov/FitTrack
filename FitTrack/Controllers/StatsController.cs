using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.ViewModels.Stats;

namespace FitTrack.Controllers;

[Authorize]
public class StatsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // ── Core counts ────────────────────────────────────────────────────────
        var totalWorkouts = await _context.Workouts.CountAsync(w => w.UserId == userId);

        var totalMinutes = await _context.Workouts
            .Where(w => w.UserId == userId)
            .SumAsync(w => (int?)w.DurationMinutes) ?? 0;

        var totalExercises = await _context.WorkoutExercises
            .CountAsync(we => we.Workout.UserId == userId);

        var workoutsThisWeek = await _context.Workouts
            .CountAsync(w => w.UserId == userId && w.Date >= startOfWeek);

        var workoutsThisMonth = await _context.Workouts
            .CountAsync(w => w.UserId == userId && w.Date >= startOfMonth);

        // ── Favourite muscle group ─────────────────────────────────────────────
        var favMuscle = await _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId)
            .GroupBy(we => we.Exercise.MuscleGroup)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        // ── Favourite exercise ─────────────────────────────────────────────────
        var favExercise = await _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId)
            .GroupBy(we => we.Exercise.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        // ── Monthly breakdown — last 6 months ─────────────────────────────────
        var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

        var rawMonthly = await _context.Workouts
            .Where(w => w.UserId == userId && w.Date >= sixMonthsAgo)
            .GroupBy(w => new { w.Date.Year, w.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count(),
                TotalMinutes = g.Sum(w => w.DurationMinutes)
            })
            .ToListAsync();

        // Fill all 6 months even if no workouts
        var monthlyData = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var date = new DateTime(now.Year, now.Month, 1).AddMonths(-5 + i);
                var found = rawMonthly.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);
                return new MonthlyWorkoutData
                {
                    Month = date.ToString("MMM yyyy"),
                    WorkoutCount = found?.Count ?? 0,
                    TotalMinutes = found?.TotalMinutes ?? 0
                };
            })
            .ToList();

        // ── Top 5 exercises by times logged ───────────────────────────────────
        var topExercises = await _context.WorkoutExercises
            .Where(we => we.Workout.UserId == userId)
            .GroupBy(we => new { we.Exercise.Name, we.Exercise.MuscleGroup })
            .Select(g => new ExerciseVolumeData
            {
                ExerciseName = g.Key.Name,
                MuscleGroup = g.Key.MuscleGroup,
                TimesLogged = g.Count(),
                TotalVolume = g.Sum(we => we.Sets * we.Reps * we.WeightKg)
            })
            .OrderByDescending(e => e.TimesLogged)
            .Take(5)
            .ToListAsync();

        // ── Detailed workout log ───────────────────────────────────────────────
        var rawWorkouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .OrderByDescending(w => w.Date)
            .ToListAsync();

        var workoutLog = rawWorkouts.Select(w => new DailyWorkoutSummary
        {
            WorkoutId = w.Id,
            Date = w.Date,
            DurationMinutes = w.DurationMinutes,
            WorkoutPlanTitle = w.WorkoutPlan?.Title,
            TotalVolume = w.WorkoutExercises
                .Where(we => we.Exercise.ExerciseType != "Cardio")
                .Sum(we => we.Sets * we.Reps * we.WeightKg),
            TotalCardioMinutes = w.WorkoutExercises
                .Where(we => we.Exercise.ExerciseType == "Cardio")
                .Sum(we => we.CardioMinutes ?? 0),
            Exercises = w.WorkoutExercises.Select(we => new WorkoutExerciseLogItem
            {
                Id = we.Id,
                ExerciseName = we.Exercise.Name,
                MuscleGroup = we.Exercise.MuscleGroup,
                ExerciseType = we.Exercise.ExerciseType,
                Sets = we.Sets,
                Reps = we.Reps,
                WeightKg = we.WeightKg,
                CardioMinutes = we.CardioMinutes,
                SpeedKmh = we.SpeedKmh,
                Incline = we.Incline
            }).ToList()
        }).ToList();

        var vm = new StatsViewModel
        {
            TotalWorkouts = totalWorkouts,
            TotalDurationMinutes = totalMinutes,
            TotalExercisesLogged = totalExercises,
            WorkoutsThisWeek = workoutsThisWeek,
            WorkoutsThisMonth = workoutsThisMonth,
            FavoriteMuscleGroup = favMuscle,
            FavoriteExercise = favExercise,
            MonthlyData = monthlyData,
            TopExercises = topExercises,
            WorkoutLog = workoutLog
        };

        return View(vm);
    }
}
