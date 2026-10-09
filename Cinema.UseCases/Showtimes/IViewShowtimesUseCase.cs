using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Showtimes;

public interface IViewShowtimesUseCase
{
    Task<IEnumerable<Showtime>> ExecuteAllAsync();
    Task<IEnumerable<Showtime>> ExecuteByDateAsync(DateTime date);
    Task<IEnumerable<Showtime>> ExecuteByMovieIdAsync(int movieId);
    Task<Showtime?> ExecuteGetByIdAsync(int showtimeId);
}
