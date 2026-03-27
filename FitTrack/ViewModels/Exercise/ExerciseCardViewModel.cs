namespace FitTrack.ViewModels.Exercise;

public class ExerciseCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = "Strength";
    public string? ImageUrl { get; set; }
}
