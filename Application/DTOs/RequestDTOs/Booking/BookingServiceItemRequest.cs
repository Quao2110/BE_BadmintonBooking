namespace Application.DTOs.RequestDTOs.Booking;

public class BookingServiceItemRequest
{
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; } = 1;
}
