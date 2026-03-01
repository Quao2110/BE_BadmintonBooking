using Application.DTOs.RequestDTOs.Category;
using Application.DTOs.ResponseDTOs.Category;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id)
            ?? throw new Exception("Category not found.");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            CategoryName = request.CategoryName
        };

        await _unitOfWork.CategoryRepository.CreateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryUpdateRequest request)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id)
            ?? throw new Exception("Category not found.");

        category.CategoryName = request.CategoryName;

        _unitOfWork.CategoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id)
            ?? throw new Exception("Category not found.");

        _unitOfWork.CategoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
