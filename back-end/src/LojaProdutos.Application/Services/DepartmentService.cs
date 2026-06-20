using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _repository.GetAllAsync();
        return departments.Select(d => new DepartmentDto { Id = d.Id, Name = d.Name }).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _repository.GetByIdAsync(id);
        return d is null ? null : new DepartmentDto { Id = d.Id, Name = d.Name };
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = new Department { Name = dto.Name };
        var created = await _repository.CreateAsync(department);
        return new DepartmentDto { Id = created.Id, Name = created.Name };
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, CreateDepartmentDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Name = dto.Name;
        var updated = await _repository.UpdateAsync(existing);
        return new DepartmentDto { Id = updated.Id, Name = updated.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
