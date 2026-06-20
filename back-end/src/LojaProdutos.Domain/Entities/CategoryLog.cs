using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

public class CategoryLog
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [MaxLength(100)]
    public string User { get; set; } = "system";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
