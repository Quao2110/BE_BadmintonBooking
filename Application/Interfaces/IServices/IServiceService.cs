using Application.DTOs.RequestDTOs.Service;
using Application.DTOs.ResponseDTOs.Service;

namespace Application.Interfaces.IServices;

public interface IServiceService
{
    Task<IEnumerable<ServiceResponse>> GetAllAsync();
    Task<ServiceResponse> GetByIdAsync(Guid id);
    Task<ServiceResponse> CreateAsync(ServiceCreateRequest request);
    Task<ServiceResponse> UpdateAsync(Guid id, ServiceUpdateRequest request);
    Task DeleteAsync(Guid id);
}
