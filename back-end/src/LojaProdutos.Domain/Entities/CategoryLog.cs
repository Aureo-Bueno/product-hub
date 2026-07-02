using System.ComponentModel.DataAnnotations;

namespace LojaProdutos.Domain.Entities;

/// <summary>
/// Represents an audit log entry recording changes made to a category.
/// </summary>
public class CategoryLog
{
    /// <summary>Unique identifier for the log entry.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key to the audited category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Navigation property to the audited category.</summary>
    public Category Category { get; set; } = null!;

    /// <summary>Type of action performed (e.g., "Created", "Updated", "Deleted"). Required, max 20 characters.</summary>
    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    /// <summary>Serialized previous values before the change, or null.</summary>
    public string? OldValues { get; set; }

    /// <summary>Serialized new values after the change, or null.</summary>
    public string? NewValues { get; set; }

    /// <summary>User who performed the action. Defaults to "system". Max 100 characters.</summary>
    [MaxLength(100)]
    public string User { get; set; } = "system";

    /// <summary>Timestamp when the log entry was created. Defaults to UTC now.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
