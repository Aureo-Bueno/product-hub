using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LojaProdutos.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core repository for <see cref="Department"/> entities with CRUD operations and referential integrity checks.
/// </summary>
public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="DepartmentRepository"/>.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all departments ordered by name.
    /// </summary>
    /// <returns>A list of all department entities.</returns>
    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments.OrderBy(d => d.Name).ToListAsync();
    }

    /// <summary>
    /// Retrieves a department by its identifier.
    /// </summary>
    /// <param name="id">Department ID.</param>
    /// <returns>The department entity, or null if not found.</returns>
    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments.FindAsync(id);
    }

    /// <summary>
    /// Creates a new department in the database.
    /// </summary>
    /// <param name="department">The department entity to create.</param>
    /// <returns>The created department with generated Id.</returns>
    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    /// <summary>
    /// Updates an existing department in the database.
    /// </summary>
    /// <param name="department">The department entity with updated values.</param>
    /// <returns>The updated department entity.</returns>
    public async Task<Department> UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        return department;
    }

    /// <summary>
    /// Deletes a department only if it has no associated categories. Prevents orphaned references.
    /// </summary>
    /// <param name="id">Department ID to delete.</param>
    /// <returns>True if the department was deleted; false if not found or has existing categories.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department is null) return false;

        var hasCategories = await _context.Categories.AnyAsync(c => c.DepartmentId == id);
        if (hasCategories) return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }
}
