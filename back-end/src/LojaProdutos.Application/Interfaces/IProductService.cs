using LojaProdutos.Application.Dtos;

namespace LojaProdutos.Application.Interfaces;

public interface IProductService
{
    Task<PaginatedResultDto<ProductResponseDto>> GetAllAsync(string? search = null, string? sortBy = null, string? order = null, int page = 1, int limit = 10);
    Task<ProductResponseDto?> GetByIdAsync(int id);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<ProductResponseDto?> ToggleFavoriteAsync(int id);
    Task<List<ProductResponseDto>> GetFavoritesAsync();
    Task<string> GenerateDescriptionAsync(string name, string? category = null);
    Task<List<string>> SuggestProductsAsync(string categoryName);
    Task<string?> ClassifyProductAsync(string text);
    Task<string> CorrectNameAsync(string name);
}
