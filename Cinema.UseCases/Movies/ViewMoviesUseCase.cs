using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Movies;

public class ViewMoviesUseCase : IViewMoviesUseCase
{
    private readonly IMovieRepository _movieRepository;

    public ViewMoviesUseCase(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<Movie>> ExecuteAsync(bool activeOnly = true)
    {
        return await _movieRepository.GetAllMoviesAsync(activeOnly);
    }

    public async Task<Movie?> GetByIdAsync(int movieId)
    {
        return await _movieRepository.GetMovieByIdAsync(movieId);
    }
}
