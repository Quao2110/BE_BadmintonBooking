namespace Application.DTOs.ResponseDTOs.Payment;

public class VnPayResultResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ResponseCode { get; set; }
    public string? TransactionStatus { get; set; }
    public string? TxnRef { get; set; }
    public string? TransactionNo { get; set; }
    public string? Amount { get; set; }
    public string? BankCode { get; set; }
    public string? PayDate { get; set; }
}
