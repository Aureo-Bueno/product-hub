namespace LojaProdutos.Application.Dtos;

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public List<CategoryResponseDto>? Children { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsFavorite { get; set; }
    public List<string> Tags { get; set; } = [];
}
