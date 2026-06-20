using LojaProdutos.Application.Dtos;
using LojaProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LojaProdutos.API.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound(new { message = "Departamento não encontrado" });
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, [FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound(new { message = "Departamento não encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return BadRequest(new { message = "Não foi possível excluir. Deparmento pode ter categorias vinculadas." });
        return Ok(new { message = "Departamento excluído" });
    }
}
