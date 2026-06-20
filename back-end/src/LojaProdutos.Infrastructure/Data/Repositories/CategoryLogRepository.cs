using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

public class CategoryLogRepository : ICategoryLogRepository
{
    private readonly AppDbContext _context;

    public CategoryLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CategoryLog log)
    {
        _context.CategoryLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CategoryLog>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.CategoryLogs
            .Where(l => l.CategoryId == categoryId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}
