using Application.DTOs.RequestDTOs.Court;
using Application.DTOs.RequestDTOs.CourtImage;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtsController : ControllerBase
{
    private readonly ICourtService _courtService;
    private readonly ICourtImageService _courtImageService;

    public CourtsController(ICourtService courtService, ICourtImageService courtImageService)
    {
        _courtService = courtService;
        _courtImageService = courtImageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _courtService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courtService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CourtCreateRequest request)
    {
        var result = await _courtService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CourtUpdateRequest request)
    {
        try
        {
            var result = await _courtService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _courtService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddImage([FromBody] CourtImageCreateRequest request)
    {
        try
        {
            var result = await _courtImageService.CreateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("images/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var result = await _courtImageService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
