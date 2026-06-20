namespace LojaProdutos.Application.Dtos;

public class ProductDescriptionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class ProductSuggestDto
{
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductClassifyDto
{
    public string Text { get; set; } = string.Empty;
}
