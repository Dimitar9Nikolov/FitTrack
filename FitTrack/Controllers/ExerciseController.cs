using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.ViewModels.Exercise;

namespace FitTrack.Controllers;

public class ExerciseController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 9;

    public ExerciseController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Exercise
    public async Task<IActionResult> Index(string? search, int? categoryId, string? type, int page = 1)
    {
        var query = _context.Exercises
            .Include(e => e.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Name.Contains(search) || e.MuscleGroup.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.ExerciseType == type);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var exercises = await query
            .OrderBy(e => e.Category.Name)
            .ThenBy(e => e.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(e => new ExerciseCardViewModel
            {
                Id           = e.Id,
                Name         = e.Name,
                MuscleGroup  = e.MuscleGroup,
                CategoryName = e.Category.Name,
                ExerciseType = e.ExerciseType,
                ImageUrl     = e.ImageUrl
            })
            .ToListAsync();

        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        return View(new ExerciseIndexViewModel
        {
            Exercises   = exercises,
            Search      = search,
            CategoryId  = categoryId,
            Type        = type,
            CurrentPage = page,
            TotalPages  = totalPages,
            TotalCount  = totalCount,
            Categories  = categories
        });
    }

    // GET: /Exercise/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var exercise = await _context.Exercises
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exercise == null) return NotFound();

        var timesLogged = await _context.WorkoutExercises
            .CountAsync(we => we.ExerciseId == id);

        return View(new ExerciseDetailsViewModel
        {
            Id                 = exercise.Id,
            Name               = exercise.Name,
            Description        = exercise.Description,
            MuscleGroup        = exercise.MuscleGroup,
            CategoryName       = exercise.Category.Name,
            ExerciseType       = exercise.ExerciseType,
            ImageUrl           = exercise.ImageUrl,
            TimesLoggedByUsers = timesLogged
        });
    }
}
