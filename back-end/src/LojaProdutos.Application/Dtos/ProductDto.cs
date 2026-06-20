using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Application.Dtos;

public class CreateProductDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public DateTime DateCreate { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public List<string>? Tags { get; set; }
}

public class UpdateProductDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public List<string>? Tags { get; set; }
}

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime DateCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsFavorite { get; set; }
    public List<string> Tags { get; set; } = [];
}
