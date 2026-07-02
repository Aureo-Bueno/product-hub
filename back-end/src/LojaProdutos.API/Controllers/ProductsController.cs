using System.Text.Json;
using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

/// <summary>
/// Controller for managing products, including CRUD, favorites,
/// and AI-powered features (description generation, suggestion,
/// classification, and name correction).
/// </summary>
[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductsController"/>.
    /// </summary>
    /// <param name="service">The product service.</param>
    public ProductsController(IProductService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product response DTO.</returns>
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _service.CreateAsync(dto));
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional search, sorting, and ordering.
    /// </summary>
    /// <param name="search">Optional search term to filter products.</param>
    /// <param name="sort">Field to sort by (default: "date").</param>
    /// <param name="order">Sort direction "asc" or "desc" (default: "desc").</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 10).</param>
    /// <returns>A paginated result containing product response DTOs.</returns>
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

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product response DTO, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound(new { message = "Produto não encontrado" });
        return Ok(dto);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID to update.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product response DTO, or 404 if not found.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound(new { message = "Produto não encontrado" });
        return Ok(updated);
    }

    /// <summary>
    /// Soft-deletes a product by setting its deleted flag.
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>A success message, or 404 if not found.</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.SoftDeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Produto não encontrado" });
        return Ok(new { message = "Produto excluído" });
    }

    /// <summary>
    /// Toggles the favorite status of a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The updated product response DTO, or 404 if not found.</returns>
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

    /// <summary>
    /// Retrieves all favorited products.
    /// </summary>
    /// <returns>A list of favorite product response DTOs.</returns>
    [HttpGet("favorites")]
    public async Task<ActionResult<List<ProductResponseDto>>> GetFavorites()
    {
        return Ok(await _service.GetFavoritesAsync());
    }

    /// <summary>
    /// Generates a product description using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the product name and optional category.</param>
    /// <returns>The generated description.</returns>
    [HttpPost("generate-description")]
    public async Task<ActionResult<string>> GenerateDescription([FromBody] ProductDescriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "O nome é obrigatório" });
        var desc = await _service.GenerateDescriptionAsync(dto.Name, dto.Category);
        return Ok(new { description = desc });
    }

    /// <summary>
    /// Streams an AI-generated product description as a server-sent event (SSE) stream.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="category">Optional product category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("generate-description-stream")]
    public async Task GenerateDescriptionStream([FromQuery] string name, [FromQuery] string? category, CancellationToken cancellationToken)
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

        await foreach (var chunk in _service.GenerateDescriptionStreamAsync(name, category, cancellationToken))
        {
            var json = JsonSerializer.Serialize(new { text = chunk });
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Suggests product names for a given category using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the category name.</param>
    /// <returns>A list of suggested product names.</returns>
    [HttpPost("suggest")]
    public async Task<ActionResult<List<string>>> Suggest([FromBody] ProductSuggestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest(new { message = "A categoria é obrigatória" });
        var suggestions = await _service.SuggestProductsAsync(dto.CategoryName);
        return Ok(new { suggestions });
    }

    /// <summary>
    /// Classifies a text into a product category using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the text to classify.</param>
    /// <returns>The matched category name, or a message if classification failed.</returns>
    [HttpPost("classify")]
    public async Task<ActionResult<string>> Classify([FromBody] ProductClassifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { message = "O texto é obrigatório" });
        var category = await _service.ClassifyProductAsync(dto.Text);
        if (category is null) return Ok(new { message = "Não foi possível classificar." });
        return Ok(new { category });
    }

    /// <summary>
    /// Corrects a product name using AI.
    /// </summary>
    /// <param name="dto">The DTO containing the original name.</param>
    /// <returns>The corrected name.</returns>
    [HttpPost("correct-name")]
    public async Task<ActionResult<string>> CorrectName([FromBody] ProductClassifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { message = "O nome é obrigatório" });
        var corrected = await _service.CorrectNameAsync(dto.Text);
        return Ok(new { corrected });
    }
}
