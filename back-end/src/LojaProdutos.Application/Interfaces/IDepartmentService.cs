using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto?> UpdateAsync(int id, CreateDepartmentDto dto);
    Task<bool> DeleteAsync(int id);
}
