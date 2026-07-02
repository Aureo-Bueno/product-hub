using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

/// <summary>
/// Application service interface for category management, including CRUD, tree, favorites, statistics, CSV export, and AI-powered features.
/// </summary>
public interface ICategoryService
{
    /// <summary>Retrieves a paginated, filtered, and sorted list of categories.</summary>
    Task<PaginatedResultDto<CategoryResponseDto>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);

    /// <summary>Gets a category by its identifier, or null if not found.</summary>
    Task<CategoryResponseDto?> GetByIdAsync(int id);

    /// <summary>Retrieves the full category hierarchy as a tree structure.</summary>
    Task<List<CategoryTreeDto>> GetTreeAsync();

    /// <summary>Creates a new category from the provided DTO and returns it.</summary>
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);

    /// <summary>Updates an existing category by identifier. Returns the updated DTO, or null if not found.</summary>
    Task<CategoryResponseDto?> UpdateAsync(int id, UpdateCategoryDto dto);

    /// <summary>Soft-deletes a category by identifier. Returns true if successful.</summary>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>Toggles the favorite status of a category. Returns the updated DTO, or null if not found.</summary>
    Task<CategoryResponseDto?> ToggleFavoriteAsync(int id);

    /// <summary>Retrieves all categories marked as favorites.</summary>
    Task<List<CategoryResponseDto>> GetFavoritesAsync();

    /// <summary>Returns aggregate category statistics.</summary>
    Task<CategoryStatsDto> GetStatsAsync();

    /// <summary>Exports categories to a CSV byte array.</summary>
    Task<byte[]> ExportCsvAsync();

    /// <summary>Uses AI to generate a description for a category given its name.</summary>
    Task<string> GenerateDescriptionAsync(string name);

    /// <summary>Streams an AI-generated description for a category, yielding tokens as they arrive.</summary>
    IAsyncEnumerable<string> GenerateDescriptionStreamAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Uses AI to suggest category names related to a given topic.</summary>
    Task<List<string>> SuggestCategoriesAsync(string topic);

    /// <summary>Uses AI to correct or normalize a category name.</summary>
    Task<string> CorrectNameAsync(string name);

    /// <summary>Checks whether a category name is a duplicate. Returns the matching category DTO, or null.</summary>
    Task<CategoryResponseDto?> CheckDuplicateAsync(string name);

    /// <summary>Uses AI to classify an arbitrary text into a category. Returns the category name, or null.</summary>
    Task<string?> ClassifyTextAsync(string text);
}
