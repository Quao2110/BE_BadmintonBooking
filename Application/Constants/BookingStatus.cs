namespace Application.Constants;

public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";

    public static readonly HashSet<string> BlockingStatuses =
    [
        Pending,
        Confirmed
    ];
}
