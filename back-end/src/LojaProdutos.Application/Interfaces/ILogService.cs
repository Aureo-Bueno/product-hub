using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

/// <summary>
/// Service interface for streaming audit log events.
/// </summary>
public interface ILogService
{
    /// <summary>Streams recent log events as they become available, yielding each event as a DTO.</summary>
    IAsyncEnumerable<LogEventDto> StreamRecentLogsAsync(CancellationToken cancellationToken = default);
}
