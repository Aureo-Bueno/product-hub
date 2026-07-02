using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

/// <summary>
/// Repository interface for Department entity CRUD operations.
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>Retrieves all departments.</summary>
    Task<List<Department>> GetAllAsync();

    /// <summary>Gets a department by its unique identifier, or null if not found.</summary>
    Task<Department?> GetByIdAsync(int id);

    /// <summary>Creates a new department and returns it.</summary>
    Task<Department> CreateAsync(Department department);

    /// <summary>Updates an existing department and returns the updated entity.</summary>
    Task<Department> UpdateAsync(Department department);

    /// <summary>Deletes a department by its identifier. Returns true if successful.</summary>
    Task<bool> DeleteAsync(int id);
}
