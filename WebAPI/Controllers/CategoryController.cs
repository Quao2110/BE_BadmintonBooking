using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Category;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(ApiResponse.Success("Categories retrieved successfully.", result));
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
            var result = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Category retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request)
    {
        try
        {
            var result = await _categoryService.CreateAsync(request);
            return Ok(ApiResponse.Success("Category created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CategoryUpdateRequest request)
    {
        try
        {
            var result = await _categoryService.UpdateAsync(id, request);
            return Ok(ApiResponse.Success("Category updated successfully.", result));
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
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Category deleted successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
