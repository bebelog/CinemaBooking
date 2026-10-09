namespace Cinema.UseCases.Movies;

public interface IDeleteMovieUseCase
{
    Task<bool> ExecuteAsync(int movieId);
}
