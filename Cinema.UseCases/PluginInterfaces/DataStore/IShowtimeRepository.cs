using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface IShowtimeRepository
{
    Task<IEnumerable<Showtime>> GetAllUpcomingShowtimesAsync();
    Task<IEnumerable<Showtime>> GetShowtimesByDateAsync(DateTime date);
    Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId);
    Task<Showtime?> GetShowtimeByIdAsync(int showtimeId);
    Task<IEnumerable<Showtime>> GetShowtimesByAuditoriumAndDateRangeAsync(int auditoriumId, DateTime start, DateTime end);
    Task<int> AddShowtimeAsync(Showtime showtime);
    Task<bool> DeleteShowtimeAsync(int showtimeId);
}
