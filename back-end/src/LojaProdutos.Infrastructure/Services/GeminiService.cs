using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LojaProdutos.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LojaProdutos.Infrastructure.Services;

/// <summary>
/// Infrastructure service that wraps Google Gemini API calls for content generation, streaming, and availability checks.
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GeminiService"/>.
    /// Reads the API key and model from the GEMINI_API_KEY and GEMINI_MODEL environment variables.
    /// </summary>
    /// <param name="httpClient">HTTP client configured for external API calls.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public GeminiService(HttpClient httpClient, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.0-flash";
    }

    /// <summary>
    /// Sends a prompt to the Gemini API and returns the generated text response.
    /// </summary>
    /// <param name="prompt">The prompt text to send.</param>
    /// <returns>The generated text, or null if the API key is missing or the call fails.</returns>
    public async Task<string?> GenerateContentAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Set GEMINI_API_KEY environment variable.");
            return null;
        }

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            var text = json?.Candidates?.FirstOrDefault()
                ?.Content?.Parts?.FirstOrDefault()
                ?.Text;

            return text?.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return null;
        }
    }

    /// <summary>
    /// Streams text chunks from the Gemini API for a given prompt using server-sent events.
    /// </summary>
    /// <param name="prompt">The prompt text to send.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the streaming operation.</param>
    /// <returns>An async enumerable of text chunks as they are received.</returns>
    public async IAsyncEnumerable<string> GenerateContentStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Set GEMINI_API_KEY environment variable.");
            yield break;
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:streamGenerateContent?alt=sse&key={_apiKey}";

        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };

        StreamReader? reader = null;

        try
        {
            var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini streaming API");
            yield break;
        }

        await foreach (var text in ReadGeminiStreamAsync(reader, cancellationToken))
        {
            yield return text;
        }
    }

    /// <summary>
    /// Reads and parses the SSE stream from the Gemini API, yielding text content from each event.
    /// </summary>
    /// <param name="reader">StreamReader over the SSE response stream.</param>
    /// <param name="cancellationToken">Cancellation token to cancel reading.</param>
    /// <returns>An async enumerable of parsed text chunks.</returns>
    private async IAsyncEnumerable<string> ReadGeminiStreamAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;

            var json = line[6..];
            if (json == "[DONE]") break;

            var text = TryParseChunk(json);
            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Attempts to parse a single SSE chunk JSON into the text content of the first candidate.
    /// </summary>
    /// <param name="json">The JSON string from the SSE data line.</param>
    /// <returns>The extracted text, or null if parsing fails.</returns>
    private string? TryParseChunk(string json)
    {
        try
        {
            var chunk = JsonSerializer.Deserialize<GeminiResponse>(json);
            return chunk?.Candidates?.FirstOrDefault()
                ?.Content?.Parts?.FirstOrDefault()
                ?.Text;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini streaming chunk");
            return null;
        }
    }

    /// <summary>
    /// Checks whether the Gemini API is reachable by issuing a GET request to the model endpoint.
    /// </summary>
    /// <returns>True if the API responds with a success status code; otherwise false.</returns>
    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) return false;

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Represents the top-level Gemini API response containing candidates.
    /// </summary>
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    /// <summary>
    /// Represents a single candidate in the Gemini API response, containing content.
    /// </summary>
    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    /// <summary>
    /// Represents the content of a Gemini candidate, containing a list of parts.
    /// </summary>
    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    /// <summary>
    /// Represents a single part of Gemini content, containing the generated text.
    /// </summary>
    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
