using System.ComponentModel.DataAnnotations;

namespace FitTrack.ViewModels.Admin;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    public int ExerciseCount { get; set; }
}
