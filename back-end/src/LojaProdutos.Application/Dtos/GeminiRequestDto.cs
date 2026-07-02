namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for a generic Gemini prompt request.
/// </summary>
public class GeminiRequestDto
{
    /// <summary>The prompt text to send to the Gemini API.</summary>
    public string Prompt { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for Gemini category suggestion results.
/// </summary>
public class GeminiCategorySuggestionDto
{
    /// <summary>List of suggested category names.</summary>
    public List<string> Suggestions { get; set; } = [];
}

/// <summary>
/// Data transfer object for requesting a Gemini-generated description for a category or product name.
/// </summary>
public class GeminiDescriptionDto
{
    /// <summary>The name of the category or product to describe.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for requesting Gemini-based text classification.
/// </summary>
public class GeminiClassificationDto
{
    /// <summary>The text to classify.</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for checking duplicate names via Gemini.
/// </summary>
public class GeminiDuplicateCheckDto
{
    /// <summary>The name to check for duplicates.</summary>
    public string Name { get; set; } = string.Empty;
}
