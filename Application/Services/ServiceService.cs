using Application.DTOs.RequestDTOs.Service;
using Application.DTOs.ResponseDTOs.Service;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ServiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceResponse>> GetAllAsync()
    {
        var services = await _unitOfWork.ServiceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ServiceResponse>>(services);
    }

    public async Task<ServiceResponse> GetByIdAsync(Guid id)
    {
        var service = await _unitOfWork.ServiceRepository.GetByIdAsync(id)
            ?? throw new Exception("Service not found.");

        return _mapper.Map<ServiceResponse>(service);
    }

    public async Task<ServiceResponse> CreateAsync(ServiceCreateRequest request)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = request.ServiceName,
            Price = request.Price,
            Unit = request.Unit,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive ?? true
        };

        await _unitOfWork.ServiceRepository.CreateAsync(service);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ServiceResponse>(service);
    }

    public async Task<ServiceResponse> UpdateAsync(Guid id, ServiceUpdateRequest request)
    {
        var service = await _unitOfWork.ServiceRepository.GetByIdAsync(id)
            ?? throw new Exception("Service not found.");

        service.ServiceName = request.ServiceName;
        service.Price = request.Price;
        service.Unit = request.Unit;
        service.StockQuantity = request.StockQuantity;
        service.IsActive = request.IsActive ?? service.IsActive;

        _unitOfWork.ServiceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ServiceResponse>(service);
    }

    public async Task DeleteAsync(Guid id)
    {
        var service = await _unitOfWork.ServiceRepository.GetByIdAsync(id)
            ?? throw new Exception("Service not found.");

        _unitOfWork.ServiceRepository.Delete(service);
        await _unitOfWork.SaveChangesAsync();
    }
}
