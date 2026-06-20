using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);
    Task<int> CountAsync(string? search = null);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(int id);
    Task<List<Product>> GetFavoritesAsync();
    Task<Product> ToggleFavoriteAsync(int id);
}
