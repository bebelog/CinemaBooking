namespace Cinema.CoreBusiness.Models;

public class Auditorium
{
    public int AuditoriumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public string ScreenType { get; set; } = "Standard 2D";
}
