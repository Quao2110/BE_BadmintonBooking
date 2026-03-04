namespace Application.DTOs.ResponseDTOs.Booking;

public class BookingAvailabilitySlotResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
