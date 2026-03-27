namespace FitTrack.ViewModels.Shared;

public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";

    /// <summary>Current filter values (excluding "page") forwarded as route data.</summary>
    public Dictionary<string, string?> RouteValues { get; set; } = new();
}
