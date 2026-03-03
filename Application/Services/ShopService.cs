using Application.DTOs.RequestDTOs.Shop;
using Application.DTOs.ResponseDTOs.Shop;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class ShopService : IShopService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShopService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ShopResponse?> GetShopInfoAsync()
    {
        var shop = await _unitOfWork.ShopRepository.GetFirstAsync();
        return _mapper.Map<ShopResponse>(shop);
    }

    public async Task<ShopResponse> UpdateShopInfoAsync(Guid id, ShopUpdateRequest request)
    {
        // Tọa độ hợp lệ: Lat -90 đến 90, Lng -180 đến 180
        if (request.Latitude < -90 || request.Latitude > 90)
            throw new Exception("Latitude must be between -90 and 90.");
        if (request.Longitude < -180 || request.Longitude > 180)
            throw new Exception("Longitude must be between -180 and 180.");

        var shop = await _unitOfWork.ShopRepository.GetByIdAsync(id);
        if (shop == null)
        {
            throw new Exception("Shop not found");
        }
        
        shop.ShopName = request.ShopName;
        shop.Address = request.Address;
        shop.Latitude = request.Latitude;
        shop.Longitude = request.Longitude;
        
        _unitOfWork.ShopRepository.Update(shop);

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ShopResponse>(shop);
    }

    public async Task<double> CalculateDistanceAsync(double userLat, double userLng)
    {
        var shop = await _unitOfWork.ShopRepository.GetFirstAsync();
        if (shop == null || !shop.Latitude.HasValue || !shop.Longitude.HasValue)
            throw new Exception("Shop location not configured.");

        var distance = HaversineDistance(userLat, userLng, shop.Latitude.Value, shop.Longitude.Value);
        return Math.Round(distance, 3);
    }

    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Bán kính Trái đất tính bằng km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
