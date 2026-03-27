using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Index(string? difficulty)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _context.WorkoutPlans
            .Include(wp => wp.Creator)
            .Include(wp => wp.Workouts)
            .Where(wp => wp.IsPublic || wp.CreatorId == userId);

        if (!string.IsNullOrEmpty(difficulty))
            query = query.Where(wp => wp.Difficulty == difficulty);

        var plans = await query
            .OrderByDescending(wp => wp.Id)
            .Select(wp => new WorkoutPlanListItemViewModel
            {
                Id = wp.Id,
                Title = wp.Title,
                Description = wp.Description,
                Difficulty = wp.Difficulty,
                IsPublic = wp.IsPublic,
                CreatorId = wp.CreatorId,
                CreatorName = wp.Creator.DisplayName,
                WorkoutCount = wp.Workouts.Count
            })
            .ToListAsync();

        ViewBag.SelectedDifficulty = difficulty;
        return View(plans);
    }

    // GET: /WorkoutPlan/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await _context.WorkoutPlans
            .Include(wp => wp.Creator)
            .Include(wp => wp.Workouts)
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
            IsOwner = plan.CreatorId == userId
        };

        return View(vm);
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
}
