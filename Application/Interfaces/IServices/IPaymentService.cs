using Application.DTOs.ResponseDTOs.Payment;
using Application.DTOs.RequestDTOs.Payment;

namespace Application.Interfaces.IServices;

public interface IPaymentService
{
    Task<CreatePaymentUrlResponse> CreateVnPayPaymentUrlAsync(Guid userId, CreateVnPayPaymentUrlRequest request, string? clientIp);
    Task<object> HandleVnPayIpnAsync(IReadOnlyDictionary<string, string> queryParams);
    Task<VnPayResultResponse> HandleVnPayReturnAsync(IReadOnlyDictionary<string, string> queryParams);
}
