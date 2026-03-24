namespace Application.DTOs.ResponseDTOs.Payment;

public class CreatePaymentUrlResponse
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = "VNPAY";
    public decimal Amount { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}
