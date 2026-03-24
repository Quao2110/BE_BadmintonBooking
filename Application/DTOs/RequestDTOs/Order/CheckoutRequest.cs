namespace Application.DTOs.RequestDTOs.Order;

public class CheckoutRequest
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public string PaymentMethod { get; set; } = "COD"; // COD, VNPAY, MOMO
}

