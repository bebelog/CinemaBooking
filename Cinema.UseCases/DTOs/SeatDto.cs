namespace Cinema.UseCases.DTOs;

public class SeatDto
{
    public int SeatId { get; set; }
    public int AuditoriumId { get; set; }
    public string RowCode { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatType { get; set; } = "Standard";
    public decimal PriceModifier { get; set; }
    public decimal FinalPrice { get; set; }
    public string SeatDisplayName => $"{RowCode}{SeatNumber}";
    public bool IsBooked { get; set; }
    public bool IsHeldByOther { get; set; }
    public bool IsHeldByMe { get; set; }
    public bool IsAvailable => !IsBooked && !IsHeldByOther;
}
