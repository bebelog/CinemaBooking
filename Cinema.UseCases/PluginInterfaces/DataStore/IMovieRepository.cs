using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllMoviesAsync(bool activeOnly = true);
    Task<Movie?> GetMovieByIdAsync(int movieId);
    Task<int> AddMovieAsync(Movie movie);
    Task<bool> UpdateMovieAsync(Movie movie);
    Task<bool> DeleteMovieAsync(int movieId);
}
