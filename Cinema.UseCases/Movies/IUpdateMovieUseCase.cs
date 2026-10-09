using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Movies;

public interface IUpdateMovieUseCase
{
    Task<bool> ExecuteAsync(Movie movie);
}
