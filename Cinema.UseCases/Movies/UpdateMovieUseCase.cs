using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Movies;

public class UpdateMovieUseCase : IUpdateMovieUseCase
{
    private readonly IMovieRepository _movieRepo;

    public UpdateMovieUseCase(IMovieRepository movieRepo)
    {
        _movieRepo = movieRepo;
    }

    public async Task<bool> ExecuteAsync(Movie movie)
    {
        if (movie.MovieId <= 0)
            throw new ArgumentException("Mã phim không hợp lệ!");

        if (string.IsNullOrWhiteSpace(movie.Title))
            throw new ArgumentException("Tên phim không được để trống!");

        if (movie.DurationMinutes <= 0)
            throw new ArgumentException("Thời lượng phim phải lớn hơn 0 phút!");

        return await _movieRepo.UpdateMovieAsync(movie);
    }
}
