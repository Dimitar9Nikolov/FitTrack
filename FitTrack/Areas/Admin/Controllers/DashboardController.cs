using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitTrack.Data;
using FitTrack.ViewModels.Admin;

namespace FitTrack.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync(),
            TotalExercises = await _context.Exercises.CountAsync(),
            TotalWorkoutPlans = await _context.WorkoutPlans.CountAsync(),
            TotalWorkouts = await _context.Workouts.CountAsync()
        };

        return View(vm);
    }
}
