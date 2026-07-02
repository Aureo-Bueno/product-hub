using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core repository for <see cref="Category"/> entities with full CRUD, search, sort, pagination, soft/hard delete, favorites, and statistics.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CategoryRepository"/>.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a paginated, searchable, and sortable list of non-deleted categories.
    /// </summary>
    /// <param name="search">Optional term to filter by name, department name, or description.</param>
    /// <param name="sortBy">Sort field (name, department, date, favorite).</param>
    /// <param name="order">Sort direction (asc or desc).</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Items per page.</param>
    /// <returns>A list of matching <see cref="Category"/> entities.</returns>
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

    /// <summary>
    /// Counts categories matching an optional search term.
    /// </summary>
    /// <param name="search">Optional term to filter by name, department name, or description.</param>
    /// <returns>The total count of matching categories.</returns>
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

    /// <summary>
    /// Retrieves a category by its ID, including deleted ones (ignores query filter).
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The category entity, or null if not found.</returns>
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Retrieves all categories including soft-deleted ones.
    /// </summary>
    /// <returns>A list of all category entities.</returns>
    public async Task<List<Category>> GetAllIncludingDeletedAsync()
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves root categories (no parent) with their department and nested children.
    /// </summary>
    /// <returns>A list of root category entities with children populated.</returns>
    public async Task<List<Category>> GetTreeAsync()
    {
        return await _context.Categories
            .Include(c => c.Department)
            .Include(c => c.Children!)
            .ThenInclude(c => c.Department)
            .Where(c => c.ParentId == null)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves direct children of a given parent category.
    /// </summary>
    /// <param name="parentId">The parent category ID.</param>
    /// <returns>A list of child category entities.</returns>
    public async Task<List<Category>> GetChildrenAsync(int parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new category in the database.
    /// </summary>
    /// <param name="category">The category entity to create.</param>
    /// <returns>The created category with generated Id.</returns>
    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// Updates an existing category in the database.
    /// </summary>
    /// <param name="category">The category entity with updated values.</param>
    /// <returns>The updated category entity.</returns>
    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// Marks a category as deleted (soft delete) by setting IsDeleted and DeletedAt.
    /// </summary>
    /// <param name="id">Category ID to soft-delete.</param>
    /// <returns>True if the category was found and marked; otherwise false.</returns>
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

    /// <summary>
    /// Permanently removes a category from the database.
    /// </summary>
    /// <param name="id">Category ID to hard-delete.</param>
    /// <returns>True if the category was found and removed; otherwise false.</returns>
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

    /// <summary>
    /// Retrieves a category by its exact name (case-insensitive), including deleted ones.
    /// </summary>
    /// <param name="name">The category name to look up.</param>
    /// <returns>The matching category entity, or null if not found.</returns>
    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Retrieves all non-deleted categories marked as favorite, ordered by name.
    /// </summary>
    /// <returns>A list of favorite category entities.</returns>
    public async Task<List<Category>> GetFavoritesAsync()
    {
        return await _context.Categories
            .Include(c => c.Department)
            .Where(c => c.IsFavorite)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Toggles the IsFavorite flag on a category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The updated category entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the category is not found.</exception>
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

    /// <summary>
    /// Computes aggregate statistics for categories: total count, created this month, and updated today.
    /// </summary>
    /// <returns>A tuple with total, createdThisMonth, and updatedToday counts.</returns>
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
