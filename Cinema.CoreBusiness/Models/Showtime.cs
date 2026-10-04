namespace Cinema.CoreBusiness.Models;

public class Showtime
{
    public int ShowtimeId { get; set; }
    public int MovieId { get; set; }
    public int AuditoriumId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public string? MovieTitle { get; set; }
    public string? MoviePosterUrl { get; set; }
    public string? MovieAgeRating { get; set; }
    public int MovieDurationMinutes { get; set; }
    public string? AuditoriumName { get; set; }
    public string? ScreenType { get; set; }
}
