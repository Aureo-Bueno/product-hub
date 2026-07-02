using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

/// <summary>
/// Repository interface for Product entity operations, including CRUD, soft delete, and favorites.
/// </summary>
public interface IProductRepository
{
    /// <summary>Retrieves a paginated, filtered, and sorted list of products.</summary>
    Task<List<Product>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);

    /// <summary>Counts products matching the optional search filter.</summary>
    Task<int> CountAsync(string? search = null);

    /// <summary>Gets a product by its unique identifier, or null if not found.</summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>Creates a new product and returns it.</summary>
    Task<Product> CreateAsync(Product product);

    /// <summary>Updates an existing product and returns the updated entity.</summary>
    Task<Product> UpdateAsync(Product product);

    /// <summary>Soft-deletes a product by marking it as deleted. Returns true if successful.</summary>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>Retrieves all products marked as favorites.</summary>
    Task<List<Product>> GetFavoritesAsync();

    /// <summary>Toggles the favorite status of a product and returns the updated entity.</summary>
    Task<Product> ToggleFavoriteAsync(int id);
}
