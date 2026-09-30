using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Movies;

public class DeleteMovieUseCase : IDeleteMovieUseCase
{
    private readonly IMovieRepository _movieRepo;

    public DeleteMovieUseCase(IMovieRepository movieRepo)
    {
        _movieRepo = movieRepo;
    }

    public async Task<bool> ExecuteAsync(int movieId)
    {
        return await _movieRepo.DeleteMovieAsync(movieId);
    }
}
