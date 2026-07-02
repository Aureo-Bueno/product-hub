using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for updating an existing category.
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>Category name. Required, minimum 5 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(5, ErrorMessage = "O nome deve ter pelo menos 5 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the category.</summary>
    public string? Description { get; set; }

    /// <summary>Foreign key to the parent department. Required.</summary>
    [Required(ErrorMessage = "O departamento é obrigatório")]
    public int DepartmentId { get; set; }

    /// <summary>Foreign key to the parent category, or null for root categories.</summary>
    public int? ParentId { get; set; }

    /// <summary>Optional list of tags to associate with the category.</summary>
    public List<string>? Tags { get; set; }
}
