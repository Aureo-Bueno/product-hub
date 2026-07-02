using System.Runtime.CompilerServices;
using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

/// <summary>
/// Application service handling product CRUD operations, AI-powered descriptions, suggestions, classification, and audit logging.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IGeminiService _gemini;
    private readonly ICategoryLogRepository _logRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductService"/>.
    /// </summary>
    /// <param name="repository">Repository for product data access.</param>
    /// <param name="categoryRepository">Repository for category data access.</param>
    /// <param name="gemini">Gemini AI service for content generation.</param>
    /// <param name="logRepository">Repository for audit logs.</param>
    public ProductService(IProductRepository repository, ICategoryRepository categoryRepository, IGeminiService gemini, ICategoryLogRepository logRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _gemini = gemini;
        _logRepository = logRepository;
    }

    /// <summary>
    /// Retrieves a paginated, searchable, sortable list of products.
    /// </summary>
    /// <param name="search">Optional search term to filter products.</param>
    /// <param name="sortBy">Field to sort by (name, price, date).</param>
    /// <param name="order">Sort direction (asc or desc).</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Number of items per page.</param>
    /// <returns>A paginated result containing product DTOs.</returns>
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

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The product DTO, or null if not found.</returns>
    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : await MapToDto(product);
    }

    /// <summary>
    /// Creates a new product and records an audit log entry.
    /// </summary>
    /// <param name="dto">Data transfer object with product creation data.</param>
    /// <returns>The created product DTO.</returns>
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

        await _logRepository.AddAsync(new CategoryLog
        {
            CategoryId = product.CategoryId,
            Action = "product_created",
            NewValues = product.Name,
            User = "Sistema"
        });

        return await MapToDto(created);
    }

    /// <summary>
    /// Updates an existing product and records an audit log entry.
    /// </summary>
    /// <param name="id">Product ID to update.</param>
    /// <param name="dto">Data transfer object with updated values.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
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

        await _logRepository.AddAsync(new CategoryLog
        {
            CategoryId = updated.CategoryId,
            Action = "product_updated",
            NewValues = updated.Name,
            User = "Sistema"
        });

        return await MapToDto(updated);
    }

    /// <summary>
    /// Soft-deletes a product and records an audit log entry.
    /// </summary>
    /// <param name="id">Product ID to delete.</param>
    /// <returns>True if the product was found and deleted; otherwise false.</returns>
    public async Task<bool> SoftDeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return false;

        var deleted = await _repository.SoftDeleteAsync(id);

        await _logRepository.AddAsync(new CategoryLog
        {
            CategoryId = existing.CategoryId,
            Action = "product_deleted",
            NewValues = existing.Name,
            User = "Sistema"
        });

        return deleted;
    }

    /// <summary>
    /// Toggles the favorite flag on a product.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The updated product DTO.</returns>
    public async Task<ProductResponseDto?> ToggleFavoriteAsync(int id)
    {
        var p = await _repository.ToggleFavoriteAsync(id);
        return await MapToDto(p);
    }

    /// <summary>
    /// Retrieves all products marked as favorite.
    /// </summary>
    /// <returns>A list of favorite product DTOs.</returns>
    public async Task<List<ProductResponseDto>> GetFavoritesAsync()
    {
        var list = await _repository.GetFavoritesAsync();
        var dtos = new List<ProductResponseDto>();
        foreach (var p in list) dtos.Add(await MapToDto(p));
        return dtos;
    }

    /// <summary>
    /// Uses Gemini AI to generate a description for a product name, optionally scoped to a category.
    /// </summary>
    /// <param name="name">Product name to describe.</param>
    /// <param name="category">Optional category context for the description.</param>
    /// <returns>The generated description text, or a fallback message if unavailable.</returns>
    public async Task<string> GenerateDescriptionAsync(string name, string? category = null)
    {
        var ctx = category is null ? "" : $" in the category '{category}'";
        var prompt = $"Generate a concise product description (in Portuguese) for '{name}'{ctx}. Return only the description text.";
        return await _gemini.GenerateContentAsync(prompt) ?? "Descrição indisponível.";
    }

    /// <summary>
    /// Streams a Gemini AI-generated description for a product name using an async enumerable.
    /// </summary>
    /// <param name="name">Product name to describe.</param>
    /// <param name="category">Optional category context for the description.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the streaming operation.</param>
    /// <returns>An async enumerable of text chunks from the AI response.</returns>
    public async IAsyncEnumerable<string> GenerateDescriptionStreamAsync(string name, string? category = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ctx = category is null ? "" : $" in the category '{category}'";
        var prompt = $"Generate a concise product description (in Portuguese) for '{name}'{ctx}. Return only the description text.";

        var hasContent = false;
        await foreach (var chunk in _gemini.GenerateContentStreamAsync(prompt, cancellationToken))
        {
            hasContent = true;
            yield return chunk;
        }

        if (!hasContent)
        {
            yield return "Descrição indisponível no momento.";
        }
    }

    /// <summary>
    /// Uses Gemini AI to suggest product names for a given category.
    /// </summary>
    /// <param name="categoryName">Category name to base suggestions on.</param>
    /// <returns>A list of suggested product name strings.</returns>
    public async Task<List<string>> SuggestProductsAsync(string categoryName)
    {
        var prompt = $"Suggest 8 product names (in Portuguese) for the category '{categoryName}'. Return only a JSON array of strings.";
        var result = await _gemini.GenerateContentAsync(prompt);
        if (string.IsNullOrWhiteSpace(result)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(result) ?? []; }
        catch { return result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(); }
    }

    /// <summary>
    /// Uses Gemini AI to classify a product description into an existing category.
    /// </summary>
    /// <param name="text">Product description to classify.</param>
    /// <returns>The name of the best-matching category.</returns>
    public async Task<string?> ClassifyProductAsync(string text)
    {
        var categories = await _categoryRepository.GetAllAsync(null, "name", "asc", 1, int.MaxValue);
        var names = string.Join(", ", categories.Select(c => c.Name));
        var prompt = $"Given these categories: [{names}]. Classify this product description into one: '{text}'. Return only the category name.";
        return await _gemini.GenerateContentAsync(prompt);
    }

    /// <summary>
    /// Uses Gemini AI to correct the spelling of a product name.
    /// </summary>
    /// <param name="name">Product name to correct.</param>
    /// <returns>The corrected name, or the original name if correction fails.</returns>
    public async Task<string> CorrectNameAsync(string name)
    {
        var prompt = $"Correct the spelling of this product name (in Portuguese): '{name}'. Return only the corrected name.";
        return await _gemini.GenerateContentAsync(prompt) ?? name;
    }

    /// <summary>
    /// Maps a <see cref="Product"/> entity to a <see cref="ProductResponseDto"/>, including the related category and department names.
    /// </summary>
    /// <param name="p">The product entity to map.</param>
    /// <returns>The mapped DTO with resolved category and department names.</returns>
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
