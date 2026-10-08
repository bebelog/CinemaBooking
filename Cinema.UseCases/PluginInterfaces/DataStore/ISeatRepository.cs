using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface ISeatRepository
{
    Task<IEnumerable<Seat>> GetSeatsByAuditoriumIdAsync(int auditoriumId);
    Task<IEnumerable<int>> GetBookedSeatIdsByShowtimeIdAsync(int showtimeId);
}
