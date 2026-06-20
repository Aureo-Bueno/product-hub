using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser positivo")]
    public decimal Price { get; set; }

    public DateTime DateCreate { get; set; }

    public DateTime? DateUpdate { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsFavorite { get; set; }

    public List<string> Tags { get; set; } = [];
}
