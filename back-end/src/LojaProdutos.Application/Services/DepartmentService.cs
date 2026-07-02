using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

/// <summary>
/// Application service handling department CRUD operations.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    /// <summary>
    /// Initializes a new instance of <see cref="DepartmentService"/>.
    /// </summary>
    /// <param name="repository">Repository for department data access.</param>
    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all departments ordered by name.
    /// </summary>
    /// <returns>A list of department DTOs.</returns>
    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _repository.GetAllAsync();
        return departments.Select(d => new DepartmentDto { Id = d.Id, Name = d.Name }).ToList();
    }

    /// <summary>
    /// Retrieves a single department by its identifier.
    /// </summary>
    /// <param name="id">Department ID.</param>
    /// <returns>The department DTO, or null if not found.</returns>
    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _repository.GetByIdAsync(id);
        return d is null ? null : new DepartmentDto { Id = d.Id, Name = d.Name };
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="dto">Data transfer object with department name.</param>
    /// <returns>The created department DTO.</returns>
    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = new Department { Name = dto.Name };
        var created = await _repository.CreateAsync(department);
        return new DepartmentDto { Id = created.Id, Name = created.Name };
    }

    /// <summary>
    /// Updates an existing department name.
    /// </summary>
    /// <param name="id">Department ID to update.</param>
    /// <param name="dto">Data transfer object with the updated name.</param>
    /// <returns>The updated department DTO, or null if not found.</returns>
    public async Task<DepartmentDto?> UpdateAsync(int id, CreateDepartmentDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Name = dto.Name;
        var updated = await _repository.UpdateAsync(existing);
        return new DepartmentDto { Id = updated.Id, Name = updated.Name };
    }

    /// <summary>
    /// Deletes a department if it has no associated categories.
    /// </summary>
    /// <param name="id">Department ID to delete.</param>
    /// <returns>True if the department was deleted; false if not found or has existing categories.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
