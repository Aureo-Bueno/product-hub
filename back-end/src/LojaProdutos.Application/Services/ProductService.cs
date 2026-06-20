using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IGeminiService _gemini;

    public ProductService(IProductRepository repository, ICategoryRepository categoryRepository, IGeminiService gemini)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _gemini = gemini;
    }

    public async Task<PaginatedResultDto<ProductResponseDto>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10)
    {
        var products = await _repository.GetAllAsync(search, sortBy, order, page, limit);
        var total = await _repository.CountAsync(search);

        var dtos = new List<ProductResponseDto>();
        foreach (var p in products)
        {
            dtos.Add(await MapToDto(p));
        }

        return new PaginatedResultDto<ProductResponseDto>
        {
            Data = dtos,
            Page = page,
            Limit = limit,
            Total = total
        };
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : await MapToDto(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DateCreate = dto.DateCreate,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags ?? []
        };

        var created = await _repository.CreateAsync(product);
        return await MapToDto(created);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.CategoryId = dto.CategoryId;
        existing.Tags = dto.Tags ?? [];
        existing.DateUpdate = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return await MapToDto(updated);
    }

    public async Task<bool> SoftDeleteAsync(int id) => await _repository.SoftDeleteAsync(id);

    public async Task<ProductResponseDto?> ToggleFavoriteAsync(int id)
    {
        var p = await _repository.ToggleFavoriteAsync(id);
        return await MapToDto(p);
    }

    public async Task<List<ProductResponseDto>> GetFavoritesAsync()
    {
        var list = await _repository.GetFavoritesAsync();
        var dtos = new List<ProductResponseDto>();
        foreach (var p in list) dtos.Add(await MapToDto(p));
        return dtos;
    }

    public async Task<string> GenerateDescriptionAsync(string name, string? category = null)
    {
        var ctx = category is null ? "" : $" in the category '{category}'";
        var prompt = $"Generate a concise product description (in Portuguese) for '{name}'{ctx}. Return only the description text.";
        return await _gemini.GenerateContentAsync(prompt) ?? "Descrição indisponível.";
    }

    public async Task<List<string>> SuggestProductsAsync(string categoryName)
    {
        var prompt = $"Suggest 8 product names (in Portuguese) for the category '{categoryName}'. Return only a JSON array of strings.";
        var result = await _gemini.GenerateContentAsync(prompt);
        if (string.IsNullOrWhiteSpace(result)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(result) ?? []; }
        catch { return result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(); }
    }

    public async Task<string?> ClassifyProductAsync(string text)
    {
        var categories = await _categoryRepository.GetAllAsync(null, "name", "asc", 1, int.MaxValue);
        var names = string.Join(", ", categories.Select(c => c.Name));
        var prompt = $"Given these categories: [{names}]. Classify this product description into one: '{text}'. Return only the category name.";
        return await _gemini.GenerateContentAsync(prompt);
    }

    public async Task<string> CorrectNameAsync(string name)
    {
        var prompt = $"Correct the spelling of this product name (in Portuguese): '{name}'. Return only the corrected name.";
        return await _gemini.GenerateContentAsync(prompt) ?? name;
    }

    private async Task<ProductResponseDto> MapToDto(Product p)
    {
        var category = await _categoryRepository.GetByIdAsync(p.CategoryId);
        return new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DateCreate = p.DateCreate,
            DateUpdate = p.DateUpdate,
            CategoryId = p.CategoryId,
            CategoryName = category?.Name ?? "",
            DepartmentName = category?.Department?.Name ?? "",
            IsDeleted = p.IsDeleted,
            DeletedAt = p.DeletedAt,
            IsFavorite = p.IsFavorite,
            Tags = p.Tags
        };
    }
}
