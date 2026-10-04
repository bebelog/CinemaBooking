using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Movies;

public class CreateMovieUseCase : ICreateMovieUseCase
{
    private readonly IMovieRepository _movieRepo;

    public CreateMovieUseCase(IMovieRepository movieRepo)
    {
        _movieRepo = movieRepo;
    }

    public async Task<int> ExecuteAsync(Movie movie)
    {
        if (string.IsNullOrWhiteSpace(movie.Title))
            throw new ArgumentException("Tên phim không được để trống!");

        if (movie.DurationMinutes <= 0)
            throw new ArgumentException("Thời lượng phim phải lớn hơn 0 phút!");

        if (string.IsNullOrWhiteSpace(movie.PosterUrl))
        {
            // Default poster placeholder if not provided
            movie.PosterUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=600&auto=format&fit=crop&q=80";
        }

        return await _movieRepo.AddMovieAsync(movie);
    }
}
