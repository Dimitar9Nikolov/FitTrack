using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.Models;
using FitTrack.ViewModels.Admin;

namespace FitTrack.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ExerciseController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExerciseController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Exercise
    public async Task<IActionResult> Index()
    {
        var exercises = await _context.Exercises
            .Include(e => e.Category)
            .OrderBy(e => e.Category.Name)
            .ThenBy(e => e.Name)
            .ToListAsync();

        return View(exercises);
    }

    // GET: Admin/Exercise/Create
    public async Task<IActionResult> Create()
    {
        var vm = new ExerciseFormViewModel
        {
            Categories = await GetCategorySelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/Exercise/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExerciseFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        _context.Exercises.Add(new Exercise
        {
            Name = vm.Name,
            Description = vm.Description,
            MuscleGroup = vm.MuscleGroup,
            ImageUrl = vm.ImageUrl,
            CategoryId = vm.CategoryId
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Exercise \"{vm.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Exercise/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        var vm = new ExerciseFormViewModel
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            MuscleGroup = exercise.MuscleGroup,
            ImageUrl = exercise.ImageUrl,
            CategoryId = exercise.CategoryId,
            Categories = await GetCategorySelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/Exercise/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExerciseFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        exercise.Name = vm.Name;
        exercise.Description = vm.Description;
        exercise.MuscleGroup = vm.MuscleGroup;
        exercise.ImageUrl = vm.ImageUrl;
        exercise.CategoryId = vm.CategoryId;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Exercise \"{vm.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Exercise/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var exercise = await _context.Exercises
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exercise == null) return NotFound();

        return View(exercise);
    }

    // POST: Admin/Exercise/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Exercise \"{exercise.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }
}
