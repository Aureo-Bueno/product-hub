namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Generic data transfer object wrapping a paginated result set with metadata.
/// </summary>
/// <typeparam name="T">Type of the items in the result set.</typeparam>
public class PaginatedResultDto<T>
{
    /// <summary>List of items for the current page.</summary>
    public List<T> Data { get; set; } = [];

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>Maximum number of items per page.</summary>
    public int Limit { get; set; }

    /// <summary>Total number of items across all pages.</summary>
    public int Total { get; set; }

    /// <summary>Total number of pages computed from Total and Limit.</summary>
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
