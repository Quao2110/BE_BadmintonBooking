using Application.DTOs.RequestDTOs.Order;
using Application.DTOs.ResponseDTOs.Common;
using Application.DTOs.ResponseDTOs.Order;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Checkout process with database transaction to ensure data consistency
    /// 1. Validate cart and delivery info
    /// 2. Check stock availability for all items
    /// 3. Create Order and OrderDetails
    /// 4. Update product stock
    /// 5. Clear user's cart
    /// </summary>
    public async Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
            throw new Exception("Delivery address is required.");

        var cart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(userId)
            ?? throw new Exception("Cart not found.");

        if (!cart.CartItems.Any())
            throw new Exception("Cart is empty. Cannot proceed with checkout.");

        // Use transaction for consistency
        try
        {
            await _unitOfWork.CommitAsync();

            // Step 1: Validate stock for all items
            var stockErrors = new List<string>();
            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product == null)
                    stockErrors.Add($"Product with ID {cartItem.ProductId} not found.");
                else if (cartItem.Product.StockQuantity < cartItem.Quantity)
                    stockErrors.Add($"Insufficient stock for {cartItem.Product.ProductName}. Available: {cartItem.Product.StockQuantity}, Requested: {cartItem.Quantity}");
            }

            if (stockErrors.Any())
                throw new Exception(string.Join(" | ", stockErrors));

            // Step 2: Create Order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeliveryAddress = request.DeliveryAddress,
                DeliveryLatitude = request.DeliveryLatitude,
                DeliveryLongitude = request.DeliveryLongitude,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending", // Pending, Paid, Failed
                OrderStatus = "Pending", // Pending, Confirmed, Shipping, Delivered, Cancelled
                OrderDate = DateTime.Now,
                TotalAmount = 0
            };

            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Step 3: Create OrderDetails and update stock
            decimal totalAmount = 0;
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(cartItem.ProductId)
                    ?? throw new Exception($"Product {cartItem.ProductId} not found.");

                var unitPrice = product.Price;
                var quantity = cartItem.Quantity ?? 0;
                var subTotal = unitPrice * quantity;

                // Create OrderDetail
                var orderDetail = new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };

                await _unitOfWork.OrderDetailRepository.CreateAsync(orderDetail);

                // Update product stock
                product.StockQuantity = (product.StockQuantity ?? 0) - quantity;
                _unitOfWork.ProductRepository.Update(product);

                totalAmount += subTotal;
            }

            // Step 4: Update Order total amount
            order.TotalAmount = totalAmount;
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            // Step 5: Clear cart
            foreach (var cartItem in cart.CartItems)
            {
                _unitOfWork.CartItemRepository.Delete(cartItem);
            }
            cart.UpdatedAt = DateTime.Now;
            _unitOfWork.CartRepository.Update(cart);
            await _unitOfWork.SaveChangesAsync();

            // Return created order
            var createdOrder = await GetOrderByIdAsync(order.Id);
            return createdOrder;
        }
        catch
        {
            throw;
        }
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdWithIncludesAsync(orderId)
            ?? throw new Exception("Order not found.");

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(Guid userId)
    {
        var orders = await _unitOfWork.OrderRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderResponse>>(orders);
    }

    public async Task<PagedResult<OrderResponse>> GetOrdersPagedAsync(
        Guid? userId,
        string? orderStatus,
        string? paymentStatus,
        int page = 1,
        int pageSize = 10)
    {
        var (orders, totalItems) = await _unitOfWork.OrderRepository.GetPagedAsync(
            userId,
            orderStatus,
            paymentStatus,
            page,
            pageSize);

        var totalPages = (totalItems + pageSize - 1) / pageSize;

        return new PagedResult<OrderResponse>
        {
            Items = _mapper.Map<IEnumerable<OrderResponse>>(orders),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, string newStatus)
    {
        var validStatuses = new[] { "Pending", "Confirmed", "Shipping", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(newStatus))
            throw new Exception($"Invalid order status. Valid statuses: {string.Join(", ", validStatuses)}");

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId)
            ?? throw new Exception("Order not found.");

        // Handle restocking if order is being cancelled
        if (newStatus == "Cancelled" && order.OrderStatus != "Cancelled")
        {
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetByOrderIdAsync(orderId);
            foreach (var detail in orderDetails)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(detail.ProductId)
                    ?? throw new Exception($"Product {detail.ProductId} not found.");

                // Restore stock
                product.StockQuantity = (product.StockQuantity ?? 0) + detail.Quantity;
                _unitOfWork.ProductRepository.Update(product);
            }
        }

        order.OrderStatus = newStatus;
        _unitOfWork.OrderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId)
            ?? throw new Exception("Order not found.");

        if (order.OrderStatus == "Cancelled")
            throw new Exception("Order is already cancelled.");

        if (order.OrderStatus == "Delivered")
            throw new Exception("Cannot cancel a delivered order.");

        // Restore stock for all items
        var orderDetails = await _unitOfWork.OrderDetailRepository.GetByOrderIdAsync(orderId);
        foreach (var detail in orderDetails)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(detail.ProductId)
                ?? throw new Exception($"Product {detail.ProductId} not found.");

            product.StockQuantity = (product.StockQuantity ?? 0) + detail.Quantity;
            _unitOfWork.ProductRepository.Update(product);
        }

        order.OrderStatus = "Cancelled";
        _unitOfWork.OrderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId);
    }
}

