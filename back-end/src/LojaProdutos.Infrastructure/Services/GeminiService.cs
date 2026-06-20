using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LojaProdutos.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LojaProdutos.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.0-flash";
    }

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

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
