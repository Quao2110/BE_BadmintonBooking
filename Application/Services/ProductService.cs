using Application.DTOs.RequestDTOs.Product;
using Application.DTOs.ResponseDTOs.Common;
using Application.DTOs.ResponseDTOs.Product;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;
namespace Application.Services;

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _unitOfWork.ProductRepository.GetAllWithIncludesAsync();
        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdWithIncludesAsync(id)
            ?? throw new Exception("Product not found.");

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductCreateRequest request)
    {
        if (request.CategoryId.HasValue)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
                throw new Exception("Category not found.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            ProductName = request.ProductName,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive ?? true
        };

        await _unitOfWork.ProductRepository.CreateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.ProductRepository.GetByIdWithIncludesAsync(product.Id) ?? product;
        return _mapper.Map<ProductResponse>(created);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, ProductUpdateRequest request)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(id)
            ?? throw new Exception("Product not found.");

        if (request.CategoryId.HasValue)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
                throw new Exception("Category not found.");
        }

        product.CategoryId = request.CategoryId;
        product.ProductName = request.ProductName;
        product.Description = request.Description;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive ?? product.IsActive;

        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.ProductRepository.GetByIdWithIncludesAsync(product.Id) ?? product;
        return _mapper.Map<ProductResponse>(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(id)
            ?? throw new Exception("Product not found.");

        _unitOfWork.ProductRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(ProductListQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 12 : Math.Min(query.PageSize, MaxPageSize);

        var (items, totalItems) = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.Search,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.Sort,
            page,
            pageSize);

        var result = new PagedResult<ProductResponse>
        {
            Items = _mapper.Map<IEnumerable<ProductResponse>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return result;
    }
}
