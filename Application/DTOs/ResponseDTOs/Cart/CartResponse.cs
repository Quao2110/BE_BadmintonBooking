namespace Application.DTOs.ResponseDTOs.Cart;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemResponse> CartItems { get; set; } = new();
    public decimal TotalPrice => CartItems.Sum(item => item.SubTotal);
}

