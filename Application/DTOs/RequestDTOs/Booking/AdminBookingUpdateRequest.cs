namespace Application.DTOs.RequestDTOs.Booking;

public class AdminBookingUpdateRequest
{
    public Guid? CourtId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Status { get; set; }
    public bool? IsPaid { get; set; }
    public decimal? TotalPrice { get; set; }
}
