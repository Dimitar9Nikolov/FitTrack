using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.Models;
using FitTrack.ViewModels.WorkoutPlan;

namespace FitTrack.Controllers;

public class WorkoutPlanController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkoutPlanController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /WorkoutPlan
    public async Task<IActionResult> Index(string? search, string? difficulty, int page = 1)
    {
        const int pageSize = 6;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _context.WorkoutPlans
            .Include(wp => wp.Creator)
            .Include(wp => wp.Workouts)
            .Include(wp => wp.PlanExercises)
            .Where(wp => wp.IsPublic || wp.CreatorId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(wp => wp.Title.Contains(search) || wp.Description.Contains(search));

        if (!string.IsNullOrWhiteSpace(difficulty))
            query = query.Where(wp => wp.Difficulty == difficulty);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var plans = await query
            .OrderByDescending(wp => wp.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(wp => new WorkoutPlanListItemViewModel
            {
                Id            = wp.Id,
                Title         = wp.Title,
                Description   = wp.Description,
                Difficulty    = wp.Difficulty,
                IsPublic      = wp.IsPublic,
                CreatorId     = wp.CreatorId,
                CreatorName   = wp.Creator.DisplayName,
                WorkoutCount  = wp.Workouts.Count,
                ExerciseCount = wp.PlanExercises.Count
            })
            .ToListAsync();

        return View(new WorkoutPlanIndexViewModel
        {
            Plans       = plans,
            Search      = search,
            Difficulty  = difficulty,
            CurrentPage = page,
            TotalPages  = totalPages,
            TotalCount  = totalCount
        });
    }

    // GET: /WorkoutPlan/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await _context.WorkoutPlans
            .Include(wp => wp.Creator)
            .Include(wp => wp.Workouts)
            .Include(wp => wp.PlanExercises.OrderBy(pe => pe.OrderIndex))
                .ThenInclude(pe => pe.Exercise)
                    .ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(wp => wp.Id == id);

        if (plan == null) return NotFound();

        // Private plan — only creator can view
        if (!plan.IsPublic && plan.CreatorId != userId)
            return NotFound();

        var vm = new WorkoutPlanDetailsViewModel
        {
            Id = plan.Id,
            Title = plan.Title,
            Description = plan.Description,
            Difficulty = plan.Difficulty,
            IsPublic = plan.IsPublic,
            CreatorId = plan.CreatorId,
            CreatorName = plan.Creator.DisplayName,
            WorkoutCount = plan.Workouts.Count,
            IsOwner = plan.CreatorId == userId,
            PlanExercises = plan.PlanExercises.Select(pe => new PlanExerciseViewModel
            {
                Id = pe.Id,
                ExerciseName = pe.Exercise.Name,
                MuscleGroup = pe.Exercise.MuscleGroup,
                CategoryName = pe.Exercise.Category.Name,
                Sets = pe.Sets,
                Reps = pe.Reps,
                Notes = pe.Notes,
                OrderIndex = pe.OrderIndex
            }).ToList()
        };

        ViewBag.ExerciseList = await GetExerciseSelectListAsync();
        return View(vm);
    }

    // POST: /WorkoutPlan/AddPlanExercise
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPlanExercise(int planId, int exerciseId, int sets, int reps, string? notes)
    {
        var plan = await _context.WorkoutPlans.FindAsync(planId);
        if (plan == null) return NotFound();
        if (!IsOwnerOrAdmin(plan.CreatorId)) return Forbid();

        var nextOrder = await _context.WorkoutPlanExercises
            .Where(pe => pe.WorkoutPlanId == planId)
            .Select(pe => (int?)pe.OrderIndex)
            .MaxAsync() ?? 0;

        _context.WorkoutPlanExercises.Add(new WorkoutPlanExercise
        {
            WorkoutPlanId = planId,
            ExerciseId = exerciseId,
            Sets = sets,
            Reps = reps,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            OrderIndex = nextOrder + 1
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = planId });
    }

    // POST: /WorkoutPlan/RemovePlanExercise
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePlanExercise(int id, int planId)
    {
        var pe = await _context.WorkoutPlanExercises
            .Include(x => x.WorkoutPlan)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pe == null) return NotFound();
        if (!IsOwnerOrAdmin(pe.WorkoutPlan.CreatorId)) return Forbid();

        _context.WorkoutPlanExercises.Remove(pe);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = planId });
    }

    // GET: /WorkoutPlan/Create
    [Authorize]
    public IActionResult Create() => View(new WorkoutPlanFormViewModel());

    // POST: /WorkoutPlan/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkoutPlanFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var plan = new WorkoutPlan
        {
            Title = vm.Title,
            Description = vm.Description,
            Difficulty = vm.Difficulty,
            IsPublic = vm.IsPublic,
            CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
        };

        _context.WorkoutPlans.Add(plan);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Workout plan \"{plan.Title}\" created!";
        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    // GET: /WorkoutPlan/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var plan = await _context.WorkoutPlans.FindAsync(id);
        if (plan == null) return NotFound();
        if (!IsOwnerOrAdmin(plan.CreatorId)) return Forbid();

        var vm = new WorkoutPlanFormViewModel
        {
            Id = plan.Id,
            Title = plan.Title,
            Description = plan.Description,
            Difficulty = plan.Difficulty,
            IsPublic = plan.IsPublic
        };
        return View(vm);
    }

    // POST: /WorkoutPlan/Edit/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkoutPlanFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var plan = await _context.WorkoutPlans.FindAsync(id);
        if (plan == null) return NotFound();
        if (!IsOwnerOrAdmin(plan.CreatorId)) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        plan.Title = vm.Title;
        plan.Description = vm.Description;
        plan.Difficulty = vm.Difficulty;
        plan.IsPublic = vm.IsPublic;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Workout plan updated successfully.";
        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    // GET: /WorkoutPlan/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _context.WorkoutPlans
            .Include(wp => wp.Creator)
            .FirstOrDefaultAsync(wp => wp.Id == id);

        if (plan == null) return NotFound();
        if (!IsOwnerOrAdmin(plan.CreatorId)) return Forbid();

        return View(new WorkoutPlanDetailsViewModel
        {
            Id = plan.Id,
            Title = plan.Title,
            Description = plan.Description,
            Difficulty = plan.Difficulty,
            IsPublic = plan.IsPublic,
            CreatorName = plan.Creator.DisplayName
        });
    }

    // POST: /WorkoutPlan/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var plan = await _context.WorkoutPlans.FindAsync(id);
        if (plan == null) return NotFound();
        if (!IsOwnerOrAdmin(plan.CreatorId)) return Forbid();

        _context.WorkoutPlans.Remove(plan);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Workout plan deleted.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsOwnerOrAdmin(string ownerId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == ownerId || User.IsInRole("Admin");
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
}
