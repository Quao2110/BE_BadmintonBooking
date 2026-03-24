namespace Application.DTOs.RequestDTOs.Payment;

public class CreateVnPayPaymentUrlRequest
{
    public int AmountVnd { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string TxnRef { get; set; } = string.Empty;
    public string? IpAddr { get; set; }
}
