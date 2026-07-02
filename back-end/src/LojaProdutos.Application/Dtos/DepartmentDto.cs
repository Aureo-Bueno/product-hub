using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for department response data.
/// </summary>
public class DepartmentDto
{
    /// <summary>Unique identifier of the department.</summary>
    public int Id { get; set; }

    /// <summary>Department name.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for creating a new department.
/// </summary>
public class CreateDepartmentDto
{
    /// <summary>Department name. Required, maximum 100 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
