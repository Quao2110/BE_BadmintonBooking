namespace Application.DTOs.ResponseDTOs.Booking;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<BookingServiceItemResponse> Services { get; set; } = [];
}
