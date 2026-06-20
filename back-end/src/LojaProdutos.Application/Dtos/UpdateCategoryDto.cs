using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(5, ErrorMessage = "O nome deve ter pelo menos 5 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "O departamento é obrigatório")]
    public int DepartmentId { get; set; }

    public int? ParentId { get; set; }

    public List<string>? Tags { get; set; }
}
