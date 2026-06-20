using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
