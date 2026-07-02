using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

/// <summary>
/// Represents a product belonging to a category, with support for soft deletion, favorites, and tags.
/// </summary>
public class Product
{
    /// <summary>Unique identifier for the product.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Product name. Required, minimum 3 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the product.</summary>
    public string? Description { get; set; }

    /// <summary>Monetary price of the product. Must be non-negative.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser positivo")]
    public decimal Price { get; set; }

    /// <summary>Date and time when the product was created.</summary>
    public DateTime DateCreate { get; set; }

    /// <summary>Date and time of the last update, or null if never updated.</summary>
    public DateTime? DateUpdate { get; set; }

    /// <summary>Foreign key to the parent category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Navigation property to the parent category.</summary>
    public Category Category { get; set; } = null!;

    /// <summary>Indicates whether the product has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Date and time when the product was soft-deleted, or null.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Indicates whether the product is marked as a favorite.</summary>
    public bool IsFavorite { get; set; }

    /// <summary>List of tags associated with the product.</summary>
    public List<string> Tags { get; set; } = [];
}
