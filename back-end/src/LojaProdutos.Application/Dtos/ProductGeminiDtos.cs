namespace LojaProdutos.Application.Dtos;

/// <summary>
/// Data transfer object for requesting a Gemini-generated product description.
/// </summary>
public class ProductDescriptionDto
{
    /// <summary>Name of the product to describe.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional category context for the description.</summary>
    public string? Category { get; set; }
}

/// <summary>
/// Data transfer object for requesting Gemini-based product suggestions given a category.
/// </summary>
public class ProductSuggestDto
{
    /// <summary>Name of the category to base suggestions on.</summary>
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for requesting Gemini-based product text classification.
/// </summary>
public class ProductClassifyDto
{
    /// <summary>The product text to classify.</summary>
    public string Text { get; set; } = string.Empty;
}
