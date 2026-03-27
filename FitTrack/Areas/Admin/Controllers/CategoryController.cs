using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.Models;
using FitTrack.ViewModels.Admin;

namespace FitTrack.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Category
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Exercises)
            .Select(c => new CategoryFormViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ExerciseCount = c.Exercises.Count
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: Admin/Category/Create
    public IActionResult Create() => View(new CategoryFormViewModel());

    // POST: Admin/Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _context.Categories.AnyAsync(c => c.Name == vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");
            return View(vm);
        }

        _context.Categories.Add(new Category { Name = vm.Name });
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Category \"{vm.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Category/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return View(new CategoryFormViewModel { Id = category.Id, Name = category.Name });
    }

    // POST: Admin/Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        if (await _context.Categories.AnyAsync(c => c.Name == vm.Name && c.Id != id))
        {
            ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");
            return View(vm);
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = vm.Name;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Category \"{vm.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Category/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Exercises)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            ExerciseCount = category.Exercises.Count
        });
    }

    // POST: Admin/Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Category \"{category.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
