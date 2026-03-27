using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.Models;
using FitTrack.ViewModels.Workout;

namespace FitTrack.Controllers;

[Authorize]
public class WorkoutController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkoutController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Workout
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
            .OrderByDescending(w => w.Date)
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

        return View(workouts);
    }

    // GET: /Workout/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();

        var workout = await _context.Workouts
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                    .ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null) return NotFound();

        var typeMap = await GetExerciseTypeMapAsync();

        var vm = new WorkoutDetailsViewModel
        {
            Id = workout.Id,
            Date = workout.Date,
            DurationMinutes = workout.DurationMinutes,
            Notes = workout.Notes,
            WorkoutPlanId = workout.WorkoutPlanId,
            WorkoutPlanTitle = workout.WorkoutPlan?.Title,
            Exercises = workout.WorkoutExercises.Select(we => new WorkoutExerciseViewModel
            {
                Id = we.Id,
                ExerciseName = we.Exercise.Name,
                MuscleGroup = we.Exercise.MuscleGroup,
                CategoryName = we.Exercise.Category.Name,
                ExerciseType = we.Exercise.ExerciseType,
                Sets = we.Sets,
                Reps = we.Reps,
                WeightKg = we.WeightKg,
                CardioMinutes = we.CardioMinutes,
                SpeedKmh = we.SpeedKmh,
                Incline = we.Incline
            }).ToList(),
            AddExerciseForm = new WorkoutExerciseFormViewModel
            {
                WorkoutId = workout.Id,
                Exercises = await GetExerciseSelectListAsync(),
                ExerciseTypeMap = typeMap
            }
        };

        return View(vm);
    }

    // GET: /Workout/Create?planId=5
    public async Task<IActionResult> Create(int? planId)
    {
        var userId = GetUserId();

        var vm = new WorkoutFormViewModel
        {
            WorkoutPlanId = planId,
            WorkoutPlans = await GetWorkoutPlanSelectListAsync(userId)
        };

        return View(vm);
    }

    // POST: /Workout/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkoutFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.WorkoutPlans = await GetWorkoutPlanSelectListAsync(GetUserId());
            return View(vm);
        }

        var workout = new Workout
        {
            UserId = GetUserId(),
            Date = vm.Date,
            DurationMinutes = vm.DurationMinutes,
            Notes = vm.Notes,
            WorkoutPlanId = vm.WorkoutPlanId
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Workout created! Now add your exercises below.";
        return RedirectToAction(nameof(Details), new { id = workout.Id });
    }

    // GET: /Workout/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
        if (workout == null) return NotFound();

        var vm = new WorkoutFormViewModel
        {
            Id = workout.Id,
            Date = workout.Date,
            DurationMinutes = workout.DurationMinutes,
            Notes = workout.Notes,
            WorkoutPlanId = workout.WorkoutPlanId,
            WorkoutPlans = await GetWorkoutPlanSelectListAsync(userId)
        };

        return View(vm);
    }

    // POST: /Workout/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkoutFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var userId = GetUserId();
        var workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
        if (workout == null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.WorkoutPlans = await GetWorkoutPlanSelectListAsync(userId);
            return View(vm);
        }

        workout.Date = vm.Date;
        workout.DurationMinutes = vm.DurationMinutes;
        workout.Notes = vm.Notes;
        workout.WorkoutPlanId = vm.WorkoutPlanId;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Workout updated successfully.";
        return RedirectToAction(nameof(Details), new { id = workout.Id });
    }

    // GET: /Workout/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var workout = await _context.Workouts
            .Include(w => w.WorkoutPlan)
            .Include(w => w.WorkoutExercises)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (workout == null) return NotFound();

        return View(new WorkoutListItemViewModel
        {
            Id = workout.Id,
            Date = workout.Date,
            DurationMinutes = workout.DurationMinutes,
            Notes = workout.Notes,
            WorkoutPlanTitle = workout.WorkoutPlan?.Title,
            ExerciseCount = workout.WorkoutExercises.Count
        });
    }

    // POST: /Workout/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetUserId();
        var workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
        if (workout == null) return NotFound();

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Workout deleted.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Workout/AddExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExercise(WorkoutExerciseFormViewModel vm)
    {
        var userId = GetUserId();
        var workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == vm.WorkoutId && w.UserId == userId);
        if (workout == null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all exercise fields correctly.";
            return RedirectToAction(nameof(Details), new { id = vm.WorkoutId });
        }

        var exercise = await _context.Exercises.FindAsync(vm.ExerciseId);
        if (exercise == null) return NotFound();

        var isCardio = exercise.ExerciseType == "Cardio";

        _context.WorkoutExercises.Add(new WorkoutExercise
        {
            WorkoutId = vm.WorkoutId,
            ExerciseId = vm.ExerciseId,
            Sets = isCardio ? 0 : vm.Sets,
            Reps = isCardio ? 0 : vm.Reps,
            WeightKg = isCardio ? 0 : vm.WeightKg,
            CardioMinutes = isCardio ? vm.CardioMinutes : null,
            SpeedKmh = isCardio ? vm.SpeedKmh : null,
            Incline = isCardio ? vm.Incline : null
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = vm.WorkoutId });
    }

    // POST: /Workout/RemoveExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveExercise(int id, int workoutId)
    {
        var userId = GetUserId();

        var we = await _context.WorkoutExercises
            .Include(x => x.Workout)
            .FirstOrDefaultAsync(x => x.Id == id && x.Workout.UserId == userId);

        if (we == null) return NotFound();

        _context.WorkoutExercises.Remove(we);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = workoutId });
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<Dictionary<int, string>> GetExerciseTypeMapAsync()
    {
        return await _context.Exercises
            .Select(e => new { e.Id, e.ExerciseType })
            .ToDictionaryAsync(e => e.Id, e => e.ExerciseType);
    }

    private async Task<IEnumerable<SelectListItem>> GetExerciseSelectListAsync()
    {
        var exercises = await _context.Exercises
            .Include(e => e.Category)
            .OrderBy(e => e.Category.Name)
            .ThenBy(e => e.Name)
            .ToListAsync();

        return exercises
            .GroupBy(e => e.Category.Name)
            .SelectMany(g => g.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} ({e.MuscleGroup})",
                Group = new SelectListGroup { Name = g.Key }
            }));
    }

    private async Task<IEnumerable<SelectListItem>> GetWorkoutPlanSelectListAsync(string userId)
    {
        return await _context.WorkoutPlans
            .Where(wp => wp.IsPublic || wp.CreatorId == userId)
            .OrderBy(wp => wp.Title)
            .Select(wp => new SelectListItem { Value = wp.Id.ToString(), Text = wp.Title })
            .ToListAsync();
    }
}
