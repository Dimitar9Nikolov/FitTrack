using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.Exercise;

public class ExerciseIndexViewModel
{
    // Results
    public List<ExerciseCardViewModel> Exercises { get; set; } = new();

    // Filters
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Type { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }

    // Dropdown data
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}
