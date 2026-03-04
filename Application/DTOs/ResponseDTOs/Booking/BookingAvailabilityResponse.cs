namespace Application.DTOs.ResponseDTOs.Booking;

public class BookingAvailabilityResponse
{
    public Guid CourtId { get; set; }
    public DateTime Date { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public int SlotMinutes { get; set; }
    public List<BookingAvailabilitySlotResponse> Slots { get; set; } = [];
}
