namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for category response data, including hierarchy, status flags, and tags.
/// </summary>
public class CategoryResponseDto
{
    /// <summary>Unique identifier of the category.</summary>
    public int Id { get; set; }

    /// <summary>Category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the category.</summary>
    public string? Description { get; set; }

    /// <summary>Date and time when the category was created.</summary>
    public DateTime DateCreate { get; set; }

    /// <summary>Date and time of the last update, or null.</summary>
    public DateTime? DateUpdate { get; set; }

    /// <summary>Foreign key to the parent department.</summary>
    public int DepartmentId { get; set; }

    /// <summary>Name of the parent department.</summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>Foreign key to the parent category, or null for root categories.</summary>
    public int? ParentId { get; set; }

    /// <summary>Child categories in the hierarchy.</summary>
    public List<CategoryResponseDto>? Children { get; set; }

    /// <summary>Indicates whether the category is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Date and time when the category was soft-deleted, or null.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Indicates whether the category is marked as a favorite.</summary>
    public bool IsFavorite { get; set; }

    /// <summary>Tags associated with the category.</summary>
    public List<string> Tags { get; set; } = [];
}
