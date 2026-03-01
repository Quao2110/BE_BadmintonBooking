using Application.DTOs.RequestDTOs.Category;
using Application.DTOs.ResponseDTOs.Category;

namespace Application.Interfaces.IServices;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<CategoryResponse> CreateAsync(CategoryCreateRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, CategoryUpdateRequest request);
    Task DeleteAsync(Guid id);
}
