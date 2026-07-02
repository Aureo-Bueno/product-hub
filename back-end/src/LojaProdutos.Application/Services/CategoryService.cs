using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

/// <summary>
/// Application service handling category CRUD operations, AI-powered descriptions, suggestions, CSV export, and audit logging.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ICategoryLogRepository _logRepository;
    private readonly IGeminiService _gemini;

    /// <summary>
    /// Initializes a new instance of <see cref="CategoryService"/>.
    /// </summary>
    /// <param name="repository">Repository for category data access.</param>
    /// <param name="logRepository">Repository for category audit logs.</param>
    /// <param name="gemini">Gemini AI service for content generation.</param>
    public CategoryService(
        ICategoryRepository repository,
        ICategoryLogRepository logRepository,
        IGeminiService gemini)
    {
        _repository = repository;
        _logRepository = logRepository;
        _gemini = gemini;
    }

    /// <summary>
    /// Retrieves a paginated, searchable, sortable list of categories.
    /// </summary>
    /// <param name="search">Optional search term to filter categories.</param>
    /// <param name="sortBy">Field to sort by (name, department, date, favorite).</param>
    /// <param name="order">Sort direction (asc or desc).</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Number of items per page.</param>
    /// <returns>A paginated result containing category DTOs.</returns>
    public async Task<PaginatedResultDto<CategoryResponseDto>> GetAllAsync(
        string? search = null, string? sortBy = null, string? order = null,
        int page = 1, int limit = 10)
    {
        var categories = await _repository.GetAllAsync(search, sortBy, order, page, limit);
        var total = await _repository.CountAsync(search);

        return new PaginatedResultDto<CategoryResponseDto>
        {
            Data = categories.Select(MapToDto).ToList(),
            Page = page,
            Limit = limit,
            Total = total
        };
    }

    /// <summary>
    /// Retrieves a single category by its identifier.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The category DTO, or null if not found.</returns>
    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    /// <summary>
    /// Builds a hierarchical tree of categories (root nodes with nested children).
    /// </summary>
    /// <returns>A list of <see cref="CategoryTreeDto"/> representing the category hierarchy.</returns>
    public async Task<List<CategoryTreeDto>> GetTreeAsync()
    {
        var all = await _repository.GetAllIncludingDeletedAsync();
        var roots = all.Where(c => c.ParentId is null && !c.IsDeleted).ToList();

        return roots.Select(c => BuildTree(c, all)).ToList();
    }

    /// <summary>
    /// Creates a new category and records an audit log entry.
    /// </summary>
    /// <param name="dto">Data transfer object with category creation data.</param>
    /// <returns>The created category DTO.</returns>
    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            DateCreate = dto.DateCreate,
            DepartmentId = dto.DepartmentId,
            ParentId = dto.ParentId,
            Tags = dto.Tags ?? []
        };

        var created = await _repository.CreateAsync(category);

        await _logRepository.AddAsync(new CategoryLog
        {
            CategoryId = created.Id,
            Action = "created",
            NewValues = JsonSerializer.Serialize(created),
            User = "system",
            CreatedAt = DateTime.UtcNow
        });

        return MapToDto(created);
    }

    /// <summary>
    /// Updates an existing category and records an audit log with old and new values.
    /// </summary>
    /// <param name="id">Category ID to update.</param>
    /// <param name="dto">Data transfer object with updated values.</param>
    /// <returns>The updated category DTO, or null if not found.</returns>
    public async Task<CategoryResponseDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        var oldValues = JsonSerializer.Serialize(existing);

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.DepartmentId = dto.DepartmentId;
        existing.ParentId = dto.ParentId;
        existing.Tags = dto.Tags ?? [];
        existing.DateUpdate = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);

        await _logRepository.AddAsync(new CategoryLog
        {
            CategoryId = updated.Id,
            Action = "updated",
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(updated),
            User = "system",
            CreatedAt = DateTime.UtcNow
        });

        return MapToDto(updated);
    }

    /// <summary>
    /// Soft-deletes a category by marking it as deleted and records an audit log.
    /// </summary>
    /// <param name="id">Category ID to delete.</param>
    /// <returns>True if the category was found and deleted; otherwise false.</returns>
    public async Task<bool> SoftDeleteAsync(int id)
    {
        var result = await _repository.SoftDeleteAsync(id);

        if (result)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category is not null)
            {
                await _logRepository.AddAsync(new CategoryLog
                {
                    CategoryId = id,
                    Action = "deleted",
                    OldValues = JsonSerializer.Serialize(category),
                    User = "system",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Toggles the favorite flag on a category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The updated category DTO, or null if not found.</returns>
    public async Task<CategoryResponseDto?> ToggleFavoriteAsync(int id)
    {
        var category = await _repository.ToggleFavoriteAsync(id);
        return category is null ? null : MapToDto(category);
    }

    /// <summary>
    /// Retrieves all categories marked as favorite.
    /// </summary>
    /// <returns>A list of favorite category DTOs.</returns>
    public async Task<List<CategoryResponseDto>> GetFavoritesAsync()
    {
        var favorites = await _repository.GetFavoritesAsync();
        return favorites.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Computes aggregate statistics about categories (total, created this month, updated today, favorites, deleted).
    /// </summary>
    /// <returns>A <see cref="CategoryStatsDto"/> with the computed statistics.</returns>
    public async Task<CategoryStatsDto> GetStatsAsync()
    {
        var (total, createdThisMonth, updatedToday) = await _repository.GetStatsAsync();

        var all = await _repository.GetAllIncludingDeletedAsync();

        return new CategoryStatsDto
        {
            Total = total,
            CreatedThisMonth = createdThisMonth,
            UpdatedToday = updatedToday,
            Favorites = all.Count(c => c.IsFavorite),
            Deleted = all.Count(c => c.IsDeleted)
        };
    }

    /// <summary>
    /// Exports all categories as a UTF-8 encoded CSV byte array.
    /// </summary>
    /// <returns>Byte array containing the CSV content.</returns>
    public async Task<byte[]> ExportCsvAsync()
    {
        var categories = await _repository.GetAllAsync(null, "name", "asc", 1, int.MaxValue);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Nome,Descrição,Departamento,Criada em,Favorita,Tags");

        foreach (var c in categories)
        {
            var tags = string.Join("; ", c.Tags);
            sb.AppendLine($"{c.Id},\"{c.Name}\",\"{c.Description}\",\"{c.Department?.Name}\",{c.DateCreate:yyyy-MM-dd},{c.IsFavorite},\"{tags}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Uses Gemini AI to generate a description for a category name.
    /// </summary>
    /// <param name="name">Category name to describe.</param>
    /// <returns>The generated description text, or a fallback message if unavailable.</returns>
    public async Task<string> GenerateDescriptionAsync(string name)
    {
        var prompt = $"Generate a concise description (in Portuguese) for a product category called '{name}'. Return only the description text, no extra formatting.";
        return await _gemini.GenerateContentAsync(prompt) ?? "Descrição indisponível no momento.";
    }

    /// <summary>
    /// Streams a Gemini AI-generated description for a category name using an async enumerable.
    /// </summary>
    /// <param name="name">Category name to describe.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the streaming operation.</param>
    /// <returns>An async enumerable of text chunks from the AI response.</returns>
    public async IAsyncEnumerable<string> GenerateDescriptionStreamAsync(string name, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var prompt = $"Generate a concise description (in Portuguese) for a product category called '{name}'. Return only the description text, no extra formatting.";

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
    /// Uses Gemini AI to suggest category names related to a given topic.
    /// </summary>
    /// <param name="topic">Topic to base suggestions on.</param>
    /// <returns>A list of suggested category name strings.</returns>
    public async Task<List<string>> SuggestCategoriesAsync(string topic)
    {
        var prompt = $"Suggest 8 category names (in Portuguese) for a topic about '{topic}'. Return only a JSON array of strings, no other text.";
        var result = await _gemini.GenerateContentAsync(prompt);

        if (string.IsNullOrWhiteSpace(result)) return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(result) ?? [];
        }
        catch
        {
            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }

    /// <summary>
    /// Uses Gemini AI to correct the spelling and grammar of a category name.
    /// </summary>
    /// <param name="name">Category name to correct.</param>
    /// <returns>The corrected name, or the original name if correction fails.</returns>
    public async Task<string> CorrectNameAsync(string name)
    {
        var prompt = $"Correct the spelling and grammar of this category name (in Portuguese): '{name}'. Return only the corrected name.";
        return await _gemini.GenerateContentAsync(prompt) ?? name;
    }

    /// <summary>
    /// Checks whether a category with the given name already exists.
    /// </summary>
    /// <param name="name">Category name to check for duplicates.</param>
    /// <returns>The existing category DTO if found, otherwise null.</returns>
    public async Task<CategoryResponseDto?> CheckDuplicateAsync(string name)
    {
        var existing = await _repository.GetByNameAsync(name);
        return existing is null ? null : MapToDto(existing);
    }

    /// <summary>
    /// Uses Gemini AI to classify a text into one of the existing categories.
    /// </summary>
    /// <param name="text">Text to classify.</param>
    /// <returns>The name of the best-matching category, or a suggested new one.</returns>
    public async Task<string?> ClassifyTextAsync(string text)
    {
        var categories = await _repository.GetAllAsync(null, "name", "asc", 1, int.MaxValue);
        var names = string.Join(", ", categories.Select(c => c.Name));

        var prompt = $"Given these existing categories: [{names}]. Classify the following text into one of them (or suggest a new one if none fits). Text: '{text}'. Return only the category name.";

        return await _gemini.GenerateContentAsync(prompt);
    }

    /// <summary>
    /// Maps a <see cref="Category"/> entity to a <see cref="CategoryResponseDto"/>.
    /// </summary>
    /// <param name="c">The category entity to map.</param>
    /// <returns>The mapped DTO.</returns>
    private static CategoryResponseDto MapToDto(Category c)
    {
        return new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            DateCreate = c.DateCreate,
            DateUpdate = c.DateUpdate,
            DepartmentId = c.DepartmentId,
            DepartmentName = c.Department?.Name ?? "",
            ParentId = c.ParentId,
            IsDeleted = c.IsDeleted,
            DeletedAt = c.DeletedAt,
            IsFavorite = c.IsFavorite,
            Tags = c.Tags
        };
    }

    /// <summary>
    /// Recursively builds a category tree node and its nested children.
    /// </summary>
    /// <param name="node">The current category node.</param>
    /// <param name="all">The full list of categories to search for children.</param>
    /// <returns>A <see cref="CategoryTreeDto"/> with nested children populated.</returns>
    private static CategoryTreeDto BuildTree(Category node, List<Category> all)
    {
        var children = all.Where(c => c.ParentId == node.Id && !c.IsDeleted).ToList();

        return new CategoryTreeDto
        {
            Id = node.Id,
            Name = node.Name,
            DepartmentId = node.DepartmentId,
            DepartmentName = node.Department?.Name ?? "",
            Children = children.Select(c => BuildTree(c, all)).ToList()
        };
    }
}
