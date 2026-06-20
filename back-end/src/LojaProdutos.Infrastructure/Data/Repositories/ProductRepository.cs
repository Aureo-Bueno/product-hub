using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

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

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.Include(p => p.Category).ThenInclude(c => c.Department).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Product>> GetFavoritesAsync()
    {
        return await _context.Products.Where(p => p.IsFavorite).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product> ToggleFavoriteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException();
        product.IsFavorite = !product.IsFavorite;
        await _context.SaveChangesAsync();
        return product;
    }
}
