using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _service.CreateAsync(dto);
        return Ok(category);
    }

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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category is null)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(category);
    }

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

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> SoftDelete(int id)
    {
        var deleted = await _service.SoftDeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(new { message = "Categoria excluída com sucesso" });
    }

    [HttpGet("tree")]
    public async Task<ActionResult<List<CategoryTreeDto>>> GetTree()
    {
        var tree = await _service.GetTreeAsync();
        return Ok(tree);
    }

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

    [HttpGet("favorites")]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetFavorites()
    {
        var favorites = await _service.GetFavoritesAsync();
        return Ok(favorites);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<CategoryStatsDto>> GetStats()
    {
        var stats = await _service.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv()
    {
        var csv = await _service.ExportCsvAsync();
        return File(csv, "text/csv", $"categorias_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("generate-description")]
    public async Task<ActionResult<string>> GenerateDescription([FromBody] GeminiDescriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "O nome é obrigatório" });

        var description = await _service.GenerateDescriptionAsync(dto.Name);
        return Ok(new { description });
    }

    [HttpPost("suggest")]
    public async Task<ActionResult<GeminiCategorySuggestionDto>> SuggestCategories([FromBody] GeminiRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return BadRequest(new { message = "O tema é obrigatório" });

        var suggestions = await _service.SuggestCategoriesAsync(dto.Prompt);
        return Ok(new GeminiCategorySuggestionDto { Suggestions = suggestions });
    }

    [HttpPost("correct-name")]
    public async Task<ActionResult<string>> CorrectName([FromBody] GeminiRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return BadRequest(new { message = "O nome é obrigatório" });

        var corrected = await _service.CorrectNameAsync(dto.Prompt);
        return Ok(new { corrected });
    }

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
