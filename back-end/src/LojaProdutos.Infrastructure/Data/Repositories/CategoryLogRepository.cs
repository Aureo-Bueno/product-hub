using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core repository for <see cref="CategoryLog"/> entities providing insert, per-category, and recent log queries.
/// </summary>
public class CategoryLogRepository : ICategoryLogRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CategoryLogRepository"/>.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public CategoryLogRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Inserts a new audit log entry into the database.
    /// </summary>
    /// <param name="log">The log entity to persist.</param>
    public async Task AddAsync(CategoryLog log)
    {
        _context.CategoryLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all log entries for a specific category, ordered by most recent first.
    /// </summary>
    /// <param name="categoryId">The category ID to filter logs by.</param>
    /// <returns>A list of matching log entities.</returns>
    public async Task<List<CategoryLog>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.CategoryLogs
            .Where(l => l.CategoryId == categoryId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the most recent log entries after a given timestamp, including the related Category, up to a specified limit.
    /// </summary>
    /// <param name="since">The minimum timestamp to filter logs from.</param>
    /// <param name="limit">Maximum number of log entries to return (default 20).</param>
    /// <returns>A list of recent log entities.</returns>
    public async Task<List<CategoryLog>> GetRecentAsync(DateTime since, int limit = 20)
    {
        return await _context.CategoryLogs
            .Include(l => l.Category)
            .Where(l => l.CreatedAt > since)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
