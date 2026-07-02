using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

/// <summary>
/// Application service interface for product management, including CRUD, favorites, and AI-powered features.
/// </summary>
public interface IProductService
{
    /// <summary>Retrieves a paginated, filtered, and sorted list of products.</summary>
    Task<PaginatedResultDto<ProductResponseDto>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);

    /// <summary>Gets a product by its identifier, or null if not found.</summary>
    Task<ProductResponseDto?> GetByIdAsync(int id);

    /// <summary>Creates a new product from the provided DTO and returns it.</summary>
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);

    /// <summary>Updates an existing product by identifier. Returns the updated DTO, or null if not found.</summary>
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto);

    /// <summary>Soft-deletes a product by identifier. Returns true if successful.</summary>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>Toggles the favorite status of a product. Returns the updated DTO, or null if not found.</summary>
    Task<ProductResponseDto?> ToggleFavoriteAsync(int id);

    /// <summary>Retrieves all products marked as favorites.</summary>
    Task<List<ProductResponseDto>> GetFavoritesAsync();

    /// <summary>Uses AI to generate a product description given its name and optional category.</summary>
    Task<string> GenerateDescriptionAsync(string name, string? category = null);

    /// <summary>Streams an AI-generated product description, yielding tokens as they arrive.</summary>
    IAsyncEnumerable<string> GenerateDescriptionStreamAsync(string name, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>Uses AI to suggest product names related to a given category name.</summary>
    Task<List<string>> SuggestProductsAsync(string categoryName);

    /// <summary>Uses AI to classify product text into a category. Returns the category name, or null.</summary>
    Task<string?> ClassifyProductAsync(string text);

    /// <summary>Uses AI to correct or normalize a product name.</summary>
    Task<string> CorrectNameAsync(string name);
}
