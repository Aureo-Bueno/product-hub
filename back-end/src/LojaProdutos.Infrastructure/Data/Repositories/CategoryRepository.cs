using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10)
    {
        var query = _context.Categories.Include(c => c.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Department.Name.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        query = (sortBy?.ToLower(), order?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(c => c.Name),
            ("name", _) => query.OrderBy(c => c.Name),
            ("department", "desc") => query.OrderByDescending(c => c.Department.Name),
            ("department", _) => query.OrderBy(c => c.Department.Name),
            ("date", "asc") => query.OrderBy(c => c.DateCreate),
            ("date", _) => query.OrderByDescending(c => c.DateCreate),
            ("favorite", _) => query.OrderByDescending(c => c.IsFavorite).ThenBy(c => c.Name),
            _ => query.OrderByDescending(c => c.DateCreate)
        };

        return await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search = null)
    {
        var query = _context.Categories.Include(c => c.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Department.Name.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        return await query.CountAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetAllIncludingDeletedAsync()
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .ToListAsync();
    }

    public async Task<List<Category>> GetTreeAsync()
    {
        return await _context.Categories
            .Include(c => c.Department)
            .Include(c => c.Children!)
            .ThenInclude(c => c.Department)
            .Where(c => c.ParentId == null)
            .ToListAsync();
    }

    public async Task<List<Category>> GetChildrenAsync(int parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return false;

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Category>> GetFavoritesAsync()
    {
        return await _context.Categories
            .Include(c => c.Department)
            .Where(c => c.IsFavorite)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> ToggleFavoriteAsync(int id)
    {
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        category.IsFavorite = !category.IsFavorite;
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<(int Total, int CreatedThisMonth, int UpdatedToday)> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var dayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var all = _context.Categories.IgnoreQueryFilters();

        var total = await all.CountAsync();
        var createdThisMonth = await all.CountAsync(c => c.DateCreate >= monthStart);
        var updatedToday = await all.CountAsync(c => c.DateUpdate >= dayStart);

        return (total, createdThisMonth, updatedToday);
    }
}
