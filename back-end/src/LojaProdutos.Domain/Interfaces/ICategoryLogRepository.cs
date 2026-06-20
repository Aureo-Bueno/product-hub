using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

public interface ICategoryLogRepository
{
    Task AddAsync(CategoryLog log);
    Task<List<CategoryLog>> GetByCategoryIdAsync(int categoryId);
}
