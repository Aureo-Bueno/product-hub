using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(5, ErrorMessage = "O nome deve ter pelo menos 5 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateCreate { get; set; }

    public DateTime? DateUpdate { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? Children { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsFavorite { get; set; }

    public List<string> Tags { get; set; } = [];

    public ICollection<CategoryLog>? Logs { get; set; }
    public ICollection<Product>? Products { get; set; }
}
