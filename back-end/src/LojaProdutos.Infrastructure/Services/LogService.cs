using System.Runtime.CompilerServices;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LojaProdutos.Infrastructure.Services;

/// <summary>
/// Infrastructure service that provides a real-time streaming feed of audit log events via async enumerable.
/// </summary>
public class LogService : ILogService
{
    private readonly ICategoryLogRepository _repository;
    private readonly ILogger<LogService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LogService"/>.
    /// </summary>
    /// <param name="repository">Repository for category log data access.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public LogService(ICategoryLogRepository repository, ILogger<LogService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Streams log events from the last 5 minutes, then polls every 3 seconds for new entries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop streaming.</param>
    /// <returns>An async enumerable of <see cref="LogEventDto"/> representing recent and live audit events.</returns>
    public async IAsyncEnumerable<LogEventDto> StreamRecentLogsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var initialSince = DateTime.UtcNow.AddMinutes(-5);
        var initialLogs = await TryPollLogsAsync(initialSince);
        if (initialLogs?.Count > 0)
        {
            foreach (var log in initialLogs.OrderBy(l => l.CreatedAt))
            {
                yield return MapToDto(log);
            }
        }

        var lastPoll = initialLogs?.Count > 0
            ? initialLogs.Max(l => l.CreatedAt)
            : DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(3000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var logs = await TryPollLogsAsync(lastPoll);
            if (logs?.Count > 0)
            {
                lastPoll = logs.Max(l => l.CreatedAt);

                foreach (var log in logs.OrderBy(l => l.CreatedAt))
                {
                    yield return MapToDto(log);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to fetch recent log entries from the repository since a given timestamp.
    /// </summary>
    /// <param name="since">The minimum <see cref="DateTime"/> to retrieve logs from.</param>
    /// <returns>A list of recent logs, or null if an error occurs.</returns>
    private async Task<List<CategoryLog>?> TryPollLogsAsync(DateTime since)
    {
        try
        {
            return await _repository.GetRecentAsync(since, 10);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error polling logs");
            return null;
        }
    }

    /// <summary>
    /// Maps a <see cref="CategoryLog"/> entity to a <see cref="LogEventDto"/> for SSE delivery.
    /// </summary>
    /// <param name="log">The log entity to map.</param>
    /// <returns>The mapped DTO with a formatted message.</returns>
    private static LogEventDto MapToDto(CategoryLog log)
    {
        var isProduct = log.Action.StartsWith("product_");
        var name = isProduct
            ? log.NewValues ?? $"Produto ID {log.Id}"
            : log.Category?.Name ?? $"ID {log.CategoryId}";
        return new LogEventDto
        {
            Id = log.Id,
            Action = log.Action,
            CategoryName = name,
            User = log.User,
            CreatedAt = log.CreatedAt,
            Message = FormatMessage(log.Action, name)
        };
    }

    /// <summary>
    /// Formats a human-readable message string in Portuguese for a given action and entity name.
    /// </summary>
    /// <param name="action">The audit action (e.g. created, updated, deleted, product_created).</param>
    /// <param name="name">The entity name to include in the message.</param>
    /// <returns>A formatted message string.</returns>
    private static string FormatMessage(string action, string name)
    {
        return action.ToLowerInvariant() switch
        {
            "created" => $"Nova categoria: {name}",
            "updated" => $"Categoria atualizada: {name}",
            "deleted" => $"Categoria excluída: {name}",
            "product_created" => $"Novo produto: {name}",
            "product_updated" => $"Produto atualizado: {name}",
            "product_deleted" => $"Produto excluído: {name}",
            _ => $"{action}: {name}"
        };
    }
}
