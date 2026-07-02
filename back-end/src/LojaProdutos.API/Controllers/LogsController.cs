using System.Text.Json;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

/// <summary>
/// Controller for streaming real-time application logs via server-sent events (SSE).
/// </summary>
[ApiController]
[Route("logs")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of <see cref="LogsController"/>.
    /// </summary>
    /// <param name="logService">The log service.</param>
    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Streams recent log entries as a server-sent event (SSE) stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var logEvent in _logService.StreamRecentLogsAsync(cancellationToken))
        {
            var json = JsonSerializer.Serialize(logEvent, _jsonOptions);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
