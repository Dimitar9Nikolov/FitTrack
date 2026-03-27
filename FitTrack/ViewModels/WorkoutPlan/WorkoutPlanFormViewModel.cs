using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.WorkoutPlan;

public class WorkoutPlanFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a difficulty.")]
    public string Difficulty { get; set; } = string.Empty;

    [Display(Name = "Make this plan public")]
    public bool IsPublic { get; set; } = true;

    public IEnumerable<SelectListItem> DifficultyOptions { get; } = new[]
    {
        new SelectListItem("Beginner", "Beginner"),
        new SelectListItem("Intermediate", "Intermediate"),
        new SelectListItem("Advanced", "Advanced")
    };
}
