namespace Cinema.CoreBusiness.Models;

public class Seat
{
    public int SeatId { get; set; }
    public int AuditoriumId { get; set; }
    public string RowCode { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatType { get; set; } = "Standard";
    public decimal PriceModifier { get; set; }

    public string SeatDisplayName => $"{RowCode}{SeatNumber}";
}
