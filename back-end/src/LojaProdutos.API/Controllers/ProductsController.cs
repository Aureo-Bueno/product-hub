using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _service.CreateAsync(dto));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<ProductResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sort = "date",
        [FromQuery] string? order = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        return Ok(await _service.GetAllAsync(search, sort, order, page, limit));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound(new { message = "Produto não encontrado" });
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound(new { message = "Produto não encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.SoftDeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Produto não encontrado" });
        return Ok(new { message = "Produto excluído" });
    }

    [HttpPost("{id:int}/favorite")]
    public async Task<ActionResult<ProductResponseDto>> ToggleFavorite(int id)
    {
        try
        {
            return Ok(await _service.ToggleFavoriteAsync(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Produto não encontrado" });
        }
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<List<ProductResponseDto>>> GetFavorites()
    {
        return Ok(await _service.GetFavoritesAsync());
    }

    [HttpPost("generate-description")]
    public async Task<ActionResult<string>> GenerateDescription([FromBody] ProductDescriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "O nome é obrigatório" });
        var desc = await _service.GenerateDescriptionAsync(dto.Name, dto.Category);
        return Ok(new { description = desc });
    }

    [HttpPost("suggest")]
    public async Task<ActionResult<List<string>>> Suggest([FromBody] ProductSuggestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest(new { message = "A categoria é obrigatória" });
        var suggestions = await _service.SuggestProductsAsync(dto.CategoryName);
        return Ok(new { suggestions });
    }

    [HttpPost("classify")]
    public async Task<ActionResult<string>> Classify([FromBody] ProductClassifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { message = "O texto é obrigatório" });
        var category = await _service.ClassifyProductAsync(dto.Text);
        if (category is null) return Ok(new { message = "Não foi possível classificar." });
        return Ok(new { category });
    }

    [HttpPost("correct-name")]
    public async Task<ActionResult<string>> CorrectName([FromBody] ProductClassifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { message = "O nome é obrigatório" });
        var corrected = await _service.CorrectNameAsync(dto.Text);
        return Ok(new { corrected });
    }
}
