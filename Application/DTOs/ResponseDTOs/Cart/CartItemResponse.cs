using Application.DTOs.ResponseDTOs.Product;

namespace Application.DTOs.ResponseDTOs.Cart;

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int? Quantity { get; set; }
    public ProductResponse? Product { get; set; }
    public decimal SubTotal => (Product?.Price ?? 0) * (Quantity ?? 0);
}

