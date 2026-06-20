using LojaProdutos.Domain.Entities;

namespace LojaProdutos.Domain.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int id);
}
