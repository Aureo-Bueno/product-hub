namespace LojaProdutos.Application.Interfaces;

public interface IGeminiService
{
    Task<string?> GenerateContentAsync(string prompt);
    Task<bool> IsAvailableAsync();
}
