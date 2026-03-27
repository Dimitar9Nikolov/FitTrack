using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
