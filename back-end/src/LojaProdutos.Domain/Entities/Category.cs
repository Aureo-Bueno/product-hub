using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

/// <summary>
/// Represents a product category within a department, supporting hierarchical parent-child relationships,
/// soft deletion, favorites, and tagging.
/// </summary>
public class Category
{
    /// <summary>Unique identifier for the category.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Category name. Required, minimum 5 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(5, ErrorMessage = "O nome deve ter pelo menos 5 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the category.</summary>
    public string? Description { get; set; }

    /// <summary>Date and time when the category was created.</summary>
    public DateTime DateCreate { get; set; }

    /// <summary>Date and time of the last update, or null if never updated.</summary>
    public DateTime? DateUpdate { get; set; }

    /// <summary>Foreign key to the parent department.</summary>
    public int DepartmentId { get; set; }

    /// <summary>Navigation property to the parent department.</summary>
    public Department Department { get; set; } = null!;

    /// <summary>Foreign key to the parent category, or null if this is a root category.</summary>
    public int? ParentId { get; set; }

    /// <summary>Navigation property to the parent category.</summary>
    public Category? Parent { get; set; }

    /// <summary>Child categories in the hierarchy.</summary>
    public ICollection<Category>? Children { get; set; }

    /// <summary>Indicates whether the category has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Date and time when the category was soft-deleted, or null.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Indicates whether the category is marked as a favorite.</summary>
    public bool IsFavorite { get; set; }

    /// <summary>List of tags associated with the category.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Audit log entries for this category.</summary>
    public ICollection<CategoryLog>? Logs { get; set; }

    /// <summary>Products belonging to this category.</summary>
    public ICollection<Product>? Products { get; set; }
}
