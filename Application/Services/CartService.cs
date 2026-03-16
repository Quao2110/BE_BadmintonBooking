using Application.DTOs.RequestDTOs.Cart;
using Application.DTOs.ResponseDTOs.Cart;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId);

        if (cart == null)
        {
            // Create new cart if not exists
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartRepository.CreateAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            var newCart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId)
                ?? throw new Exception("Failed to create cart.");
            return _mapper.Map<CartResponse>(newCart);
        }

        return _mapper.Map<CartResponse>(cart);
    }

    public async Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            throw new Exception("Quantity must be greater than 0.");

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId)
            ?? throw new Exception("Product not found.");

        if (product.StockQuantity < request.Quantity)
            throw new Exception("Insufficient stock for this product.");

        var cart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartRepository.CreateAsync(cart);
            var result = await _unitOfWork.SaveChangesAsync();
        }

        // Check if product already in cart
        var existingItem = await _unitOfWork.CartItemRepository.GetByCartIdAndProductIdAsync(cart.Id, request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity = (existingItem.Quantity ?? 0) + request.Quantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            await _unitOfWork.CartItemRepository.CreateAsync(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var updatedCart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId)
            ?? throw new Exception("Failed to add to cart.");
        return _mapper.Map<CartResponse>(updatedCart);
    }

    public async Task<CartResponse> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new Exception("Quantity must be greater than 0.");

        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId)
            ?? throw new Exception("Cart item not found.");

        var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartItem.CartId)
            ?? throw new Exception("Cart not found.");

        if (cart.UserId != userId)
            throw new Exception("Unauthorized action.");

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(cartItem.ProductId)
            ?? throw new Exception("Product not found.");

        if (product.StockQuantity < request.Quantity)
            throw new Exception("Insufficient stock for this product.");

        cartItem.Quantity = request.Quantity;

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var updatedCart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId)
            ?? throw new Exception("Failed to update cart.");
        return _mapper.Map<CartResponse>(updatedCart);
    }

    public async Task DeleteCartItemAsync(Guid userId, Guid cartItemId)
    {
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId)
            ?? throw new Exception("Cart item not found.");

        var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartItem.CartId)
            ?? throw new Exception("Cart not found.");

        if (cart.UserId != userId)
            throw new Exception("Unauthorized action.");

        _unitOfWork.CartItemRepository.Delete(cartItem);

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId)
            ?? throw new Exception("Cart not found.");

        foreach (var item in cart.CartItems)
        {
            _unitOfWork.CartItemRepository.Delete(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }
}

