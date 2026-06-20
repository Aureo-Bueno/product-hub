using System.Globalization;
using System.Text;
using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using LojaProdutos.Domain.Entities;
using LojaProdutos.Domain.Interfaces;

namespace LojaProdutos.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ICategoryLogRepository _logRepository;
    private readonly IGeminiService _gemini;

    public CategoryService(
        ICategoryRepository repository,
        ICategoryLogRepository logRepository,
        IGeminiService gemini)
    {
        _repository = repository;
        _logRepository = logRepository;
        _gemini = gemini;
    }

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

    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<List<CategoryTreeDto>> GetTreeAsync()
    {
        var all = await _repository.GetAllIncludingDeletedAsync();
        var roots = all.Where(c => c.ParentId is null && !c.IsDeleted).ToList();

        return roots.Select(c => BuildTree(c, all)).ToList();
    }

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

    public async Task<CategoryResponseDto?> ToggleFavoriteAsync(int id)
    {
        var category = await _repository.ToggleFavoriteAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<List<CategoryResponseDto>> GetFavoritesAsync()
    {
        var favorites = await _repository.GetFavoritesAsync();
        return favorites.Select(MapToDto).ToList();
    }

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

    public async Task<string> GenerateDescriptionAsync(string name)
    {
        var prompt = $"Generate a concise description (in Portuguese) for a product category called '{name}'. Return only the description text, no extra formatting.";
        return await _gemini.GenerateContentAsync(prompt) ?? "Descrição indisponível no momento.";
    }

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

    public async Task<string> CorrectNameAsync(string name)
    {
        var prompt = $"Correct the spelling and grammar of this category name (in Portuguese): '{name}'. Return only the corrected name.";
        return await _gemini.GenerateContentAsync(prompt) ?? name;
    }

    public async Task<CategoryResponseDto?> CheckDuplicateAsync(string name)
    {
        var existing = await _repository.GetByNameAsync(name);
        return existing is null ? null : MapToDto(existing);
    }

    public async Task<string?> ClassifyTextAsync(string text)
    {
        var categories = await _repository.GetAllAsync(null, "name", "asc", 1, int.MaxValue);
        var names = string.Join(", ", categories.Select(c => c.Name));

        var prompt = $"Given these existing categories: [{names}]. Classify the following text into one of them (or suggest a new one if none fits). Text: '{text}'. Return only the category name.";

        return await _gemini.GenerateContentAsync(prompt);
    }

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
