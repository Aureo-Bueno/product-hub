using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

/// <summary>
/// Controller for managing product categories, including CRUD, tree retrieval,
/// favorites, statistics, CSV export, and AI-powered features (description generation,
/// suggestion, classification, and duplicate checking).
/// </summary>
[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="CategoriesController"/>.
    /// </summary>
    /// <param name="service">The category service.</param>
    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <returns>The created category response DTO.</returns>
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _service.CreateAsync(dto);
        return Ok(category);
    }

    /// <summary>
    /// Retrieves a paginated list of categories with optional search, sorting, and ordering.
    /// </summary>
    /// <param name="search">Optional search term to filter categories.</param>
    /// <param name="sort">Field to sort by (default: "date").</param>
    /// <param name="order">Sort direction "asc" or "desc" (default: "desc").</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 10).</param>
    /// <returns>A paginated result containing category response DTOs.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<CategoryResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sort = "date",
        [FromQuery] string? order = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var result = await _service.GetAllAsync(search, sort, order, page, limit);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a category by its ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The category response DTO, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category is null)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(category);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="dto">The updated category data.</param>
    /// <returns>The updated category response DTO, or 404 if not found.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _service.UpdateAsync(id, dto);

        if (category is null)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(category);
    }

    /// <summary>
    /// Soft-deletes a category by setting its deleted flag.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>A success message, or 404 if not found.</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> SoftDelete(int id)
    {
        var deleted = await _service.SoftDeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(new { message = "Categoria excluída com sucesso" });
    }

    /// <summary>
    /// Retrieves the category hierarchy as a tree structure.
    /// </summary>
    /// <returns>A list of category tree DTOs.</returns>
    [HttpGet("tree")]
    public async Task<ActionResult<List<CategoryTreeDto>>> GetTree()
    {
        var tree = await _service.GetTreeAsync();
        return Ok(tree);
    }

    /// <summary>
    /// Toggles the favorite status of a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The updated category response DTO, or 404 if not found.</returns>
    [HttpPost("{id:int}/favorite")]
    public async Task<ActionResult<CategoryResponseDto>> ToggleFavorite(int id)
    {
        try
        {
            var category = await _service.ToggleFavoriteAsync(id);
            return Ok(category);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Categoria não encontrada" });
        }
    }

    /// <summary>
    /// Retrieves all favorited categories.
    /// </summary>
    /// <returns>A list of favorite category response DTOs.</returns>
    [HttpGet("favorites")]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetFavorites()
    {
        var favorites = await _service.GetFavoritesAsync();
        return Ok(favorites);
    }

    /// <summary>
    /// Retrieves aggregate statistics about categories.
    /// </summary>
    /// <returns>A category stats DTO.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<CategoryStatsDto>> GetStats()
    {
        var stats = await _service.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Exports all categories as a CSV file.
    /// </summary>
    /// <returns>A CSV file download.</returns>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv()
    {
        var csv = await _service.ExportCsvAsync();
        return File(csv, "text/csv", $"categorias_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Generates a description for a category name using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the category name.</param>
    /// <returns>The generated description.</returns>
    [HttpPost("generate-description")]
    public async Task<ActionResult<string>> GenerateDescription([FromBody] GeminiDescriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "O nome é obrigatório" });

        var description = await _service.GenerateDescriptionAsync(dto.Name);
        return Ok(new { description });
    }

    /// <summary>
    /// Streams an AI-generated description for a category name as a server-sent event (SSE) stream.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("generate-description-stream")]
    public async Task GenerateDescriptionStream([FromQuery] string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("data: {\"error\":\"Name is required\"}\n\n", cancellationToken);
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var chunk in _service.GenerateDescriptionStreamAsync(name, cancellationToken))
        {
            var json = JsonSerializer.Serialize(new { text = chunk });
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Suggests category names based on a given theme using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the theme prompt.</param>
    /// <returns>A list of suggested category names.</returns>
    [HttpPost("suggest")]
    public async Task<ActionResult<GeminiCategorySuggestionDto>> SuggestCategories([FromBody] GeminiRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return BadRequest(new { message = "O tema é obrigatório" });

        var suggestions = await _service.SuggestCategoriesAsync(dto.Prompt);
        return Ok(new GeminiCategorySuggestionDto { Suggestions = suggestions });
    }

    /// <summary>
    /// Corrects a category name using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the original name.</param>
    /// <returns>The corrected name.</returns>
    [HttpPost("correct-name")]
    public async Task<ActionResult<string>> CorrectName([FromBody] GeminiRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return BadRequest(new { message = "O nome é obrigatório" });

        var corrected = await _service.CorrectNameAsync(dto.Prompt);
        return Ok(new { corrected });
    }

    /// <summary>
    /// Checks whether a category name already exists (duplicate detection).
    /// </summary>
    /// <param name="dto">The DTO containing the name to check.</param>
    /// <returns>A result indicating whether a duplicate was found and the matching category.</returns>
    [HttpPost("check-duplicate")]
    public async Task<ActionResult<CategoryResponseDto?>> CheckDuplicate([FromBody] GeminiDuplicateCheckDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "O nome é obrigatório" });

        var duplicate = await _service.CheckDuplicateAsync(dto.Name);

        if (duplicate is null)
            return Ok(new { isDuplicate = false, category = (object?)null });

        return Ok(new { isDuplicate = true, category = duplicate });
    }

    /// <summary>
    /// Classifies a text into an existing category using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the text to classify.</param>
    /// <returns>The matched category name, or a message if classification failed.</returns>
    [HttpPost("classify")]
    public async Task<ActionResult<string>> ClassifyText([FromBody] GeminiClassificationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { message = "O texto é obrigatório" });

        var category = await _service.ClassifyTextAsync(dto.Text);

        if (category is null)
            return Ok(new { message = "Não foi possível classificar o texto." });

        return Ok(new { category });
    }
}
