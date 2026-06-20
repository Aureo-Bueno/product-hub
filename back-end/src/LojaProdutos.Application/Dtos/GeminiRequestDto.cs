namespace LojaProdutos.Application.Dtos;

public class GeminiRequestDto
{
    public string Prompt { get; set; } = string.Empty;
}

public class GeminiCategorySuggestionDto
{
    public List<string> Suggestions { get; set; } = [];
}

public class GeminiDescriptionDto
{
    public string Name { get; set; } = string.Empty;
}

public class GeminiClassificationDto
{
    public string Text { get; set; } = string.Empty;
}

public class GeminiDuplicateCheckDto
{
    public string Name { get; set; } = string.Empty;
}
