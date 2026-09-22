namespace Cinema.CoreBusiness.Models;

public class Ticket
{
    public int TicketId { get; set; }
    public int BookingId { get; set; }
    public int ShowtimeId { get; set; }
    public int SeatId { get; set; }
    public decimal TicketPrice { get; set; }
    public string TicketCode { get; set; } = string.Empty;

    public string? SeatDisplayName { get; set; }
    public string? SeatType { get; set; }
}
