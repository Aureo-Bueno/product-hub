using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for creating a new product.
/// </summary>
public class CreateProductDto
{
    /// <summary>Product name. Required, minimum 3 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    public string? Description { get; set; }

    /// <summary>Monetary price. Must be non-negative.</summary>
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>Date and time when the product was created.</summary>
    public DateTime DateCreate { get; set; }

    /// <summary>Foreign key to the parent category. Required.</summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>Optional list of tags to associate with the product.</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing product.
/// </summary>
public class UpdateProductDto
{
    /// <summary>Product name. Required, minimum 3 characters.</summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    public string? Description { get; set; }

    /// <summary>Monetary price. Must be non-negative.</summary>
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>Foreign key to the parent category. Required.</summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>Optional list of tags to associate with the product.</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Data transfer object for product response data, including category and department names, status flags, and tags.
/// </summary>
public class ProductResponseDto
{
    /// <summary>Unique identifier of the product.</summary>
    public int Id { get; set; }

    /// <summary>Product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    public string? Description { get; set; }

    /// <summary>Monetary price.</summary>
    public decimal Price { get; set; }

    /// <summary>Date and time when the product was created.</summary>
    public DateTime DateCreate { get; set; }

    /// <summary>Date and time of the last update, or null.</summary>
    public DateTime? DateUpdate { get; set; }

    /// <summary>Foreign key to the parent category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Name of the parent category.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Name of the department associated via the parent category.</summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>Indicates whether the product is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Date and time when the product was soft-deleted, or null.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Indicates whether the product is marked as a favorite.</summary>
    public bool IsFavorite { get; set; }

    /// <summary>Tags associated with the product.</summary>
    public List<string> Tags { get; set; } = [];
}
