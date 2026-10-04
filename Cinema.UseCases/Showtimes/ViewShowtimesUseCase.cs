using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Showtimes;

public class ViewShowtimesUseCase : IViewShowtimesUseCase
{
    private readonly IShowtimeRepository _showtimeRepository;

    public ViewShowtimesUseCase(IShowtimeRepository showtimeRepository)
    {
        _showtimeRepository = showtimeRepository;
    }

    public async Task<IEnumerable<Showtime>> ExecuteAllAsync()
    {
        return await _showtimeRepository.GetAllUpcomingShowtimesAsync();
    }

    public async Task<IEnumerable<Showtime>> ExecuteByDateAsync(DateTime date)
    {
        return await _showtimeRepository.GetShowtimesByDateAsync(date);
    }

    public async Task<IEnumerable<Showtime>> ExecuteByMovieIdAsync(int movieId)
    {
        return await _showtimeRepository.GetShowtimesByMovieIdAsync(movieId);
    }

    public async Task<Showtime?> ExecuteGetByIdAsync(int showtimeId)
    {
        return await _showtimeRepository.GetShowtimeByIdAsync(showtimeId);
    }
}
