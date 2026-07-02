namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object representing a single audit log event for streaming.
/// </summary>
public class LogEventDto
{
    /// <summary>Unique identifier of the log entry.</summary>
    public int Id { get; set; }

    /// <summary>Type of action performed (e.g., "Created", "Updated", "Deleted").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Name of the category associated with the event.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>User who performed the action.</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>Timestamp when the event occurred.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Human-readable message describing the event.</summary>
    public string Message { get; set; } = string.Empty;
}
