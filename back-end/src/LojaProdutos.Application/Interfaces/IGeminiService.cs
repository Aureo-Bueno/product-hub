namespace LojaProdutos.Application.Interfaces;

/// <summary>
/// Service interface for interacting with the Google Gemini AI API for content generation.
/// </summary>
public interface IGeminiService
{
    /// <summary>Sends a prompt to Gemini and returns the generated text response, or null on failure.</summary>
    Task<string?> GenerateContentAsync(string prompt);

    /// <summary>Sends a prompt to Gemini and streams the generated text response token by token.</summary>
    IAsyncEnumerable<string> GenerateContentStreamAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>Checks whether the Gemini API service is currently available and configured.</summary>
    Task<bool> IsAvailableAsync();
}
