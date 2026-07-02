using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

/// <summary>
/// Application service interface for department CRUD operations.
/// </summary>
public interface IDepartmentService
{
    /// <summary>Retrieves all departments.</summary>
    Task<List<DepartmentDto>> GetAllAsync();

    /// <summary>Gets a department by its identifier, or null if not found.</summary>
    Task<DepartmentDto?> GetByIdAsync(int id);

    /// <summary>Creates a new department from the provided DTO and returns it.</summary>
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);

    /// <summary>Updates an existing department by identifier. Returns the updated DTO, or null if not found.</summary>
    Task<DepartmentDto?> UpdateAsync(int id, CreateDepartmentDto dto);

    /// <summary>Deletes a department by identifier. Returns true if successful.</summary>
    Task<bool> DeleteAsync(int id);
}
