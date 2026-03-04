namespace Application.Options;

public class BookingOptions
{
    public decimal CourtHourlyRate { get; set; }
    public string OpenTime { get; set; } = "06:00";
    public string CloseTime { get; set; } = "22:00";
    public int SlotMinutes { get; set; } = 60;
}
