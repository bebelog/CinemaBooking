namespace Cinema.UseCases.DTOs;

public class OccupancyReportDto
{
    public int ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string AuditoriumName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public decimal OccupancyRate => TotalSeats > 0 ? Math.Round((decimal)BookedSeats / TotalSeats * 100, 1) : 0;
    public decimal TotalRevenue { get; set; }
}
