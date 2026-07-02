using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

/// <summary>
/// Repository interface for Category entity operations, including CRUD, tree traversal, soft/hard delete, favorites, and statistics.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>Retrieves a paginated, filtered, and sorted list of categories.</summary>
    Task<List<Category>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);

    /// <summary>Counts categories matching the optional search filter.</summary>
    Task<int> CountAsync(string? search = null);

    /// <summary>Gets a category by its unique identifier, or null if not found.</summary>
    Task<Category?> GetByIdAsync(int id);

    /// <summary>Retrieves all categories including soft-deleted ones.</summary>
    Task<List<Category>> GetAllIncludingDeletedAsync();

    /// <summary>Retrieves the full category hierarchy as a tree.</summary>
    Task<List<Category>> GetTreeAsync();

    /// <summary>Retrieves direct child categories of a given parent category.</summary>
    Task<List<Category>> GetChildrenAsync(int parentId);

    /// <summary>Creates a new category and returns it.</summary>
    Task<Category> CreateAsync(Category category);

    /// <summary>Updates an existing category and returns the updated entity.</summary>
    Task<Category> UpdateAsync(Category category);

    /// <summary>Soft-deletes a category by marking it as deleted. Returns true if successful.</summary>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>Permanently removes a category from the database. Returns true if successful.</summary>
    Task<bool> HardDeleteAsync(int id);

    /// <summary>Searches for a category by its exact name, or null if not found.</summary>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>Retrieves all categories marked as favorites.</summary>
    Task<List<Category>> GetFavoritesAsync();

    /// <summary>Toggles the favorite status of a category and returns the updated entity.</summary>
    Task<Category> ToggleFavoriteAsync(int id);

    /// <summary>Returns aggregate statistics: total count, created this month, and updated today.</summary>
    Task<(int Total, int CreatedThisMonth, int UpdatedToday)> GetStatsAsync();
}
