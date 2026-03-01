using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Service;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/services")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _serviceService.GetAllAsync();
            return Ok(ApiResponse.Success("Services retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _serviceService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Service retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceCreateRequest request)
    {
        try
        {
            var result = await _serviceService.CreateAsync(request);
            return Ok(ApiResponse.Success("Service created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ServiceUpdateRequest request)
    {
        try
        {
            var result = await _serviceService.UpdateAsync(id, request);
            return Ok(ApiResponse.Success("Service updated successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            await _serviceService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Service deleted successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
