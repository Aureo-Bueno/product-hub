using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core repository for <see cref="Product"/> entities with full CRUD, search, sort, pagination, soft delete, and favorites.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductRepository"/>.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a paginated, searchable, and sortable list of non-deleted products.
    /// </summary>
    /// <param name="search">Optional term to filter by name or description.</param>
    /// <param name="sortBy">Sort field (name, price, date).</param>
    /// <param name="order">Sort direction (asc or desc).</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Items per page.</param>
    /// <returns>A list of matching <see cref="Product"/> entities with Category and Department included.</returns>
    public async Task<List<Product>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10)
    {
        var query = _context.Products.Include(p => p.Category).ThenInclude(c => c.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        query = (sortBy?.ToLower(), order?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("name", _) => query.OrderBy(p => p.Name),
            ("price", "desc") => query.OrderByDescending(p => p.Price),
            ("price", _) => query.OrderBy(p => p.Price),
            ("date", "asc") => query.OrderBy(p => p.DateCreate),
            ("date", _) => query.OrderByDescending(p => p.DateCreate),
            _ => query.OrderByDescending(p => p.DateCreate)
        };

        return await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
    }

    /// <summary>
    /// Counts products matching an optional search term.
    /// </summary>
    /// <param name="search">Optional term to filter by name or description.</param>
    /// <returns>The total count of matching products.</returns>
    public async Task<int> CountAsync(string? search = null)
    {
        var query = _context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || (p.Description != null && p.Description.ToLower().Contains(term)));
        }
        return await query.CountAsync();
    }

    /// <summary>
    /// Retrieves a product by its ID with its Category and Department included.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The product entity, or null if not found.</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.Include(p => p.Category).ThenInclude(c => c.Department).FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Creates a new product in the database.
    /// </summary>
    /// <param name="product">The product entity to create.</param>
    /// <returns>The created product with generated Id.</returns>
    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Updates an existing product in the database.
    /// </summary>
    /// <param name="product">The product entity with updated values.</param>
    /// <returns>The updated product entity.</returns>
    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Marks a product as deleted (soft delete) by setting IsDeleted and DeletedAt.
    /// </summary>
    /// <param name="id">Product ID to soft-delete.</param>
    /// <returns>True if the product was found and marked; otherwise false.</returns>
    public async Task<bool> SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Retrieves all non-deleted products marked as favorite, ordered by name.
    /// </summary>
    /// <returns>A list of favorite product entities.</returns>
    public async Task<List<Product>> GetFavoritesAsync()
    {
        return await _context.Products.Where(p => p.IsFavorite).OrderBy(p => p.Name).ToListAsync();
    }

    /// <summary>
    /// Toggles the IsFavorite flag on a product.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The updated product entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the product is not found.</exception>
    public async Task<Product> ToggleFavoriteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException();
        product.IsFavorite = !product.IsFavorite;
        await _context.SaveChangesAsync();
        return product;
    }
}
