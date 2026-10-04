using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Movies;

public interface IViewMoviesUseCase
{
    Task<IEnumerable<Movie>> ExecuteAsync(bool activeOnly = true);
    Task<Movie?> GetByIdAsync(int movieId);
}
