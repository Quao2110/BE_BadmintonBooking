using Application.DTOs.RequestDTOs.Cart;
using Application.DTOs.ResponseDTOs.Cart;

namespace Application.Interfaces.IServices;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(Guid userId);
    Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<CartResponse> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
    Task DeleteCartItemAsync(Guid userId, Guid cartItemId);
    Task ClearCartAsync(Guid userId);
}

