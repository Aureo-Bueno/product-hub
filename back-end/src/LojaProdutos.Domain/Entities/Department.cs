using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Category>? Categories { get; set; }
}
