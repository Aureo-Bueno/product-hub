namespace LojaProdutos.Application.Dtos;

public class CategoryTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public List<CategoryTreeDto> Children { get; set; } = [];
}
