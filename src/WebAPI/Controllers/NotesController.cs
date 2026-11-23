using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// /api/notes adresine gelen istekleri karşılar.
/// Controller'ın tek görevi: isteği al → Business'a ver → sonucu HTTP cevabına çevir.
/// İş kuralı (validation, cache...) burada YAZILMAZ.
/// </summary>
[ApiController]
[Route("api/[controller]")]            // [controller] = sınıf adından "Controller" atılmış hali: notes
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    // GET /api/notes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _noteService.GetAllAsync();
        return Ok(result);
    }

    // GET /api/notes/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _noteService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST /api/notes      gövde: { "title": "...", "content": "..." }
    [HttpPost]
    public async Task<IActionResult> Add(NoteCreateDto dto)
    {
        var result = await _noteService.AddAsync(dto);
        if (!result.Success)
            return BadRequest(result);                                    // 400

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);  // 201
    }

    // PUT /api/notes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, NoteCreateDto dto)
    {
        var result = await _noteService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/notes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _noteService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
