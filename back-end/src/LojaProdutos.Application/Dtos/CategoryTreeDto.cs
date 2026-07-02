namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object representing a node in the category hierarchy tree.
/// </summary>
public class CategoryTreeDto
{
    /// <summary>Unique identifier of the category.</summary>
    public int Id { get; set; }

    /// <summary>Category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Foreign key to the parent department.</summary>
    public int DepartmentId { get; set; }

    /// <summary>Name of the parent department.</summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>Child category nodes.</summary>
    public List<CategoryTreeDto> Children { get; set; } = [];
}
