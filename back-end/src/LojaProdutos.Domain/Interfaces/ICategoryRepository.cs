using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);
    Task<int> CountAsync(string? search = null);
    Task<Category?> GetByIdAsync(int id);
    Task<List<Category>> GetAllIncludingDeletedAsync();
    Task<List<Category>> GetTreeAsync();
    Task<List<Category>> GetChildrenAsync(int parentId);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> SoftDeleteAsync(int id);
    Task<bool> HardDeleteAsync(int id);
    Task<Category?> GetByNameAsync(string name);
    Task<List<Category>> GetFavoritesAsync();
    Task<Category> ToggleFavoriteAsync(int id);
    Task<(int Total, int CreatedThisMonth, int UpdatedToday)> GetStatsAsync();
}
