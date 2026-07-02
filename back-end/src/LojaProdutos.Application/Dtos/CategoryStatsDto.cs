namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for aggregate category statistics.
/// </summary>
public class CategoryStatsDto
{
    /// <summary>Total number of categories.</summary>
    public int Total { get; set; }

    /// <summary>Number of categories created in the current month.</summary>
    public int CreatedThisMonth { get; set; }

    /// <summary>Number of categories updated today.</summary>
    public int UpdatedToday { get; set; }

    /// <summary>Number of categories marked as favorites.</summary>
    public int Favorites { get; set; }

    /// <summary>Number of soft-deleted categories.</summary>
    public int Deleted { get; set; }
}
