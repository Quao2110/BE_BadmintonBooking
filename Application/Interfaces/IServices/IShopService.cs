using Application.DTOs.RequestDTOs.Shop;
using Application.DTOs.ResponseDTOs.Shop;

namespace Application.Interfaces.IServices;

public interface IShopService
{
    Task<ShopResponse?> GetShopInfoAsync();
    Task<List<ShopResponse>> GetAllShopsAsync();
    Task<ShopResponse> UpdateShopInfoAsync(Guid id, ShopUpdateRequest request);
    Task<double> CalculateDistanceAsync(double userLat, double userLng);
}
