using Cinema.CoreBusiness.Models;
using Cinema.UseCases.AI;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Xunit;

namespace Cinema.UnitTests;

/// <summary>
/// Kiểm thử tính năng Mở Rộng: AI Semantic Search phim & Trợ lý gợi ý suất chiếu
/// </summary>
public class AiSemanticSearchTests
{
    private readonly FakeMovieRepository _movieRepo;
    private readonly FakeShowtimeRepository _showtimeRepo;
    private readonly SemanticMovieSearchUseCase _useCase;

    public AiSemanticSearchTests()
    {
        var sampleMovies = new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "Mai",
                Genre = "Tâm lý, Tình cảm",
                AgeRating = "T18",
                DurationMinutes = 131,
                Description = "Câu chuyện tình yêu nhiều trắc trở và nước mắt của Mai và Dương."
            },
            new()
            {
                MovieId = 2,
                Title = "Kung Fu Panda 4",
                Genre = "Hoạt hình, Hài, Phiêu lưu",
                AgeRating = "P",
                DurationMinutes = 94,
                Description = "Po bước vào vai trò Thủ lĩnh tinh thần của Thung lũng Bình Yên và gặp gỡ gia đình mới."
            },
            new()
            {
                MovieId = 3,
                Title = "Godzilla x Kong: Đế Chế Mới",
                Genre = "Hành động, Phiêu lưu, Viễn tưởng",
                AgeRating = "T13",
                DurationMinutes = 115,
                Description = "Hai titan hùng mạnh đối đầu với mối đe dọa khổng lồ ẩn sâu trong Trái Đất."
            }
        };

        var today = DateTime.Today;
        var sampleShowtimes = new List<Showtime>
        {
            new() { ShowtimeId = 101, MovieId = 2, StartTime = today.AddHours(10), BasePrice = 85000 },
            new() { ShowtimeId = 102, MovieId = 2, StartTime = today.AddHours(19), BasePrice = 95000 },
            new() { ShowtimeId = 103, MovieId = 3, StartTime = today.AddHours(20), BasePrice = 110000 }
        };

        _movieRepo = new FakeMovieRepository(sampleMovies);
        _showtimeRepo = new FakeShowtimeRepository(sampleShowtimes);
        _useCase = new SemanticMovieSearchUseCase(_movieRepo, _showtimeRepo);
    }

    [Fact]
    public async Task Search_WhenQueryFamilyGentle_PrioritizesKungFuPanda()
    {
        // Act: Tìm kiếm ngữ nghĩa "phim nhẹ nhàng cho gia đình"
        var results = await _useCase.ExecuteAsync("phim nhẹ nhàng cho gia đình");

        // Assert
        Assert.NotEmpty(results);
        var topPick = results.First();
        Assert.Equal("Kung Fu Panda 4", topPick.Movie.Title);
        Assert.Contains("P", topPick.MatchReason);
    }

    [Fact]
    public async Task Search_WhenQueryActionBlockbuster_PrioritizesGodzillaKong()
    {
        // Act: Tìm kiếm "bom tấn hành động kịch tính"
        var results = await _useCase.ExecuteAsync("bom tấn hành động kịch tính");

        // Assert
        Assert.NotEmpty(results);
        var topPick = results.First();
        Assert.Equal("Godzilla x Kong: Đế Chế Mới", topPick.Movie.Title);
    }

    [Fact]
    public async Task Search_WhenQueryEveningTime_FiltersEveningShowtimes()
    {
        // Act: Tìm phim gia đình vào "tối nay"
        var results = await _useCase.ExecuteAsync("phim gia đình tối nay");

        // Assert
        Assert.NotEmpty(results);
        var topPick = results.First();
        Assert.Contains(topPick.SuggestedShowtimes, s => s.StartTime.Hour >= 18);
    }

    private class FakeMovieRepository : IMovieRepository
    {
        private readonly List<Movie> _movies;
        public FakeMovieRepository(List<Movie> movies) => _movies = movies;
        public Task<IEnumerable<Movie>> GetAllMoviesAsync(bool activeOnly = true) => Task.FromResult<IEnumerable<Movie>>(_movies);
        public Task<Movie?> GetMovieByIdAsync(int movieId) => Task.FromResult(_movies.FirstOrDefault(m => m.MovieId == movieId));
        public Task<int> AddMovieAsync(Movie movie) => Task.FromResult(movie.MovieId);
        public Task<bool> UpdateMovieAsync(Movie movie) => Task.FromResult(true);
        public Task<bool> DeleteMovieAsync(int movieId) => Task.FromResult(true);
    }

    private class FakeShowtimeRepository : IShowtimeRepository
    {
        private readonly List<Showtime> _showtimes;
        public FakeShowtimeRepository(List<Showtime> showtimes) => _showtimes = showtimes;
        public Task<IEnumerable<Showtime>> GetAllUpcomingShowtimesAsync() => Task.FromResult<IEnumerable<Showtime>>(_showtimes);
        public Task<IEnumerable<Showtime>> GetShowtimesByDateAsync(DateTime date) => Task.FromResult(_showtimes.Where(s => s.StartTime.Date == date.Date));
        public Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId) => Task.FromResult(_showtimes.Where(s => s.MovieId == movieId));
        public Task<Showtime?> GetShowtimeByIdAsync(int showtimeId) => Task.FromResult(_showtimes.FirstOrDefault(s => s.ShowtimeId == showtimeId));
        public Task<IEnumerable<Showtime>> GetShowtimesByAuditoriumAndDateRangeAsync(int auditoriumId, DateTime start, DateTime end) => Task.FromResult<IEnumerable<Showtime>>(_showtimes);
        public Task<int> AddShowtimeAsync(Showtime showtime) => Task.FromResult(showtime.ShowtimeId);
        public Task<bool> DeleteShowtimeAsync(int showtimeId) => Task.FromResult(true);
    }
}
