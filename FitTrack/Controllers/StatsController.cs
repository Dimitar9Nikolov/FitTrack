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
        var startOfWeek  = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // ── Load all workouts with exercises + sets in one query ───────────────
        var allWorkouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.ExerciseSets)
            .OrderByDescending(w => w.Date)
            .ToListAsync();

        // ── Core counts ────────────────────────────────────────────────────────
        var totalWorkouts    = allWorkouts.Count;
        var totalMinutes     = allWorkouts.Sum(w => w.DurationMinutes);
        var totalExercises   = allWorkouts.Sum(w => w.WorkoutExercises.Count);
        var workoutsThisWeek  = allWorkouts.Count(w => w.Date >= startOfWeek);
        var workoutsThisMonth = allWorkouts.Count(w => w.Date >= startOfMonth);

        // ── Favourite muscle group ─────────────────────────────────────────────
        var favMuscle = allWorkouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => we.Exercise.MuscleGroup)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // ── Favourite exercise ─────────────────────────────────────────────────
        var favExercise = allWorkouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => we.Exercise.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // ── Monthly breakdown — last 6 months ─────────────────────────────────
        var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

        var monthlyData = Enumerable.Range(0, 6).Select(i =>
        {
            var date     = new DateTime(now.Year, now.Month, 1).AddMonths(-5 + i);
            var monthly  = allWorkouts.Where(w => w.Date.Year == date.Year && w.Date.Month == date.Month).ToList();
            return new MonthlyWorkoutData
            {
                Month         = date.ToString("MMM yyyy"),
                WorkoutCount  = monthly.Count,
                TotalMinutes  = monthly.Sum(w => w.DurationMinutes)
            };
        }).ToList();

        // ── Top 5 exercises by times logged ───────────────────────────────────
        var topExercises = allWorkouts
            .SelectMany(w => w.WorkoutExercises)
            .GroupBy(we => new { we.Exercise.Name, we.Exercise.MuscleGroup })
            .Select(g => new ExerciseVolumeData
            {
                ExerciseName = g.Key.Name,
                MuscleGroup  = g.Key.MuscleGroup,
                TimesLogged  = g.Count(),
                TotalVolume  = g.Sum(we => we.ExerciseSets.Sum(s => s.Reps * s.WeightKg))
            })
            .OrderByDescending(e => e.TimesLogged)
            .Take(5)
            .ToList();

        // ── Detailed workout log ───────────────────────────────────────────────
        var workoutLog = allWorkouts.Select(w => new DailyWorkoutSummary
        {
            WorkoutId        = w.Id,
            Date             = w.Date,
            DurationMinutes  = w.DurationMinutes,
            WorkoutPlanTitle = w.WorkoutPlan?.Title,
            TotalVolume = w.WorkoutExercises
                .Where(we => we.Exercise.ExerciseType != "Cardio")
                .Sum(we => we.ExerciseSets.Sum(s => s.Reps * s.WeightKg)),
            TotalCardioMinutes = w.WorkoutExercises
                .Where(we => we.Exercise.ExerciseType == "Cardio")
                .Sum(we => we.CardioMinutes ?? 0),
            Exercises = w.WorkoutExercises.Select(we => new WorkoutExerciseLogItem
            {
                Id           = we.Id,
                ExerciseName = we.Exercise.Name,
                MuscleGroup  = we.Exercise.MuscleGroup,
                ExerciseType = we.Exercise.ExerciseType,
                Sets = we.ExerciseSets
                    .OrderBy(s => s.SetNumber)
                    .Select(s => new WorkoutSetLogItem
                    {
                        SetNumber = s.SetNumber,
                        Reps      = s.Reps,
                        WeightKg  = s.WeightKg
                    }).ToList(),
                CardioMinutes = we.CardioMinutes,
                SpeedKmh      = we.SpeedKmh,
                Incline       = we.Incline
            }).ToList()
        }).ToList();

        var vm = new StatsViewModel
        {
            TotalWorkouts          = totalWorkouts,
            TotalDurationMinutes   = totalMinutes,
            TotalExercisesLogged   = totalExercises,
            WorkoutsThisWeek       = workoutsThisWeek,
            WorkoutsThisMonth      = workoutsThisMonth,
            FavoriteMuscleGroup    = favMuscle,
            FavoriteExercise       = favExercise,
            MonthlyData            = monthlyData,
            TopExercises           = topExercises,
            WorkoutLog             = workoutLog
        };

        return View(vm);
    }
}
