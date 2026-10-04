namespace Cinema.CoreBusiness.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int ShowtimeId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime BookingTime { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
    public string PaymentMethod { get; set; } = "VNPAY / QR";

    public List<Ticket> Tickets { get; set; } = new();
}
