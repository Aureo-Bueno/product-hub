using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

/// <summary>
/// Repository interface for reading and writing category audit log entries.
/// </summary>
public interface ICategoryLogRepository
{
    /// <summary>Persists a new category log entry.</summary>
    Task AddAsync(CategoryLog log);

    /// <summary>Retrieves all log entries for a specific category, ordered by most recent.</summary>
    Task<List<CategoryLog>> GetByCategoryIdAsync(int categoryId);

    /// <summary>Retrieves recent log entries created after the given timestamp, up to the specified limit.</summary>
    Task<List<CategoryLog>> GetRecentAsync(DateTime since, int limit = 20);
}
