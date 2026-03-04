namespace Application.DTOs.RequestDTOs.Booking;

public class BookingCreateRequest
{
    public Guid CourtId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<BookingServiceItemRequest>? ServiceItems { get; set; }
}
