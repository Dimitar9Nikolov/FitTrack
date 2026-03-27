using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitTrack.ViewModels.Admin;

public class ExerciseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Muscle group is required.")]
    [MaxLength(100, ErrorMessage = "Muscle group cannot exceed 100 characters.")]
    [Display(Name = "Muscle Group")]
    public string MuscleGroup { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}
