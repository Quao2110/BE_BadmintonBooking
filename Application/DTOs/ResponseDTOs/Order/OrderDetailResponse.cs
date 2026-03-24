using Application.DTOs.ResponseDTOs.Product;

namespace Application.DTOs.ResponseDTOs.Order;

public class OrderDetailResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
    public ProductResponse? Product { get; set; }
}

