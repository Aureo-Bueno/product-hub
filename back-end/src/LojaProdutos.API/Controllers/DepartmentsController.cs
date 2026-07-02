using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

/// <summary>
/// Controller for managing departments (top-level organizational units
/// that contain categories).
/// </summary>
[ApiController]
[Route("departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="DepartmentsController"/>.
    /// </summary>
    /// <param name="service">The department service.</param>
    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all departments.
    /// </summary>
    /// <returns>A list of department DTOs.</returns>
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// Retrieves a department by its ID.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <returns>The department DTO, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound(new { message = "Departamento não encontrado" });
        return Ok(dto);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="dto">The department creation data.</param>
    /// <returns>The created department DTO.</returns>
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="id">The department ID to update.</param>
    /// <param name="dto">The updated department data.</param>
    /// <returns>The updated department DTO, or 404 if not found.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound(new { message = "Departamento não encontrado" });
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a department. Fails if the department still has linked categories.
    /// </summary>
    /// <param name="id">The department ID to delete.</param>
    /// <returns>A success message, or 400 if deletion is not possible.</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return BadRequest(new { message = "Não foi possível excluir. Deparmento pode ter categorias vinculadas." });
        return Ok(new { message = "Departamento excluído" });
    }
}
