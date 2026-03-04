namespace Application.DTOs.ResponseDTOs.Booking;

public class BookingServiceItemResponse
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtBooking { get; set; }
    public decimal LineTotal => PriceAtBooking * Quantity;
}
