using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

public interface ICategoryService
{
    Task<PaginatedResultDto<CategoryResponseDto>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);
    Task<CategoryResponseDto?> GetByIdAsync(int id);
    Task<List<CategoryTreeDto>> GetTreeAsync();
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryResponseDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<CategoryResponseDto?> ToggleFavoriteAsync(int id);
    Task<List<CategoryResponseDto>> GetFavoritesAsync();
    Task<CategoryStatsDto> GetStatsAsync();
    Task<byte[]> ExportCsvAsync();
    Task<string> GenerateDescriptionAsync(string name);
    Task<List<string>> SuggestCategoriesAsync(string topic);
    Task<string> CorrectNameAsync(string name);
    Task<CategoryResponseDto?> CheckDuplicateAsync(string name);
    Task<string?> ClassifyTextAsync(string text);
}
