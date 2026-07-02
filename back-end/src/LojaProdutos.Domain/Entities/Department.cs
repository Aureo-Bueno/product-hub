using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

/// <summary>
/// Represents a top-level department that groups related categories.
/// </summary>
public class Department
{
    /// <summary>Unique identifier for the department.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Department name. Required, maximum 100 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Categories belonging to this department.</summary>
    public ICollection<Category>? Categories { get; set; }
}
