using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Movies;

public interface ICreateMovieUseCase
{
    Task<int> ExecuteAsync(Movie movie);
}
