using System.Data;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Dapper;

namespace Cinema.DataStore.SQL.Dapper.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ISqlConnectionFactory _factory;

    public MovieRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync(bool activeOnly = true)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT MovieId, Title, Director, Cast, Genre, DurationMinutes, ReleaseDate, AgeRating, PosterUrl, TrailerUrl, Description, IsActive
            FROM dbo.Movie
            WHERE (@ActiveOnly = 0 OR IsActive = 1)
            ORDER BY MovieId DESC";

        var result = await connection.QueryAsync<Movie>(sql, new { ActiveOnly = activeOnly ? 1 : 0 });
        return result.ToList();
    }

    public async Task<Movie?> GetMovieByIdAsync(int movieId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT MovieId, Title, Director, Cast, Genre, DurationMinutes, ReleaseDate, AgeRating, PosterUrl, TrailerUrl, Description, IsActive
            FROM dbo.Movie
            WHERE MovieId = @MovieId";

        return await connection.QueryFirstOrDefaultAsync<Movie>(sql, new { MovieId = movieId });
    }

    public async Task<int> AddMovieAsync(Movie movie)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Movie (Title, Director, Cast, Genre, DurationMinutes, ReleaseDate, AgeRating, PosterUrl, TrailerUrl, Description, IsActive)
            VALUES (@Title, @Director, @Cast, @Genre, @DurationMinutes, @ReleaseDate, @AgeRating, @PosterUrl, @TrailerUrl, @Description, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, movie);
    }

    public async Task<bool> UpdateMovieAsync(Movie movie)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Movie
            SET Title = @Title, Director = @Director, Cast = @Cast, Genre = @Genre,
                DurationMinutes = @DurationMinutes, ReleaseDate = @ReleaseDate,
                AgeRating = @AgeRating, PosterUrl = @PosterUrl, TrailerUrl = @TrailerUrl,
                Description = @Description, IsActive = @IsActive
            WHERE MovieId = @MovieId";

        var rows = await connection.ExecuteAsync(sql, movie);
        return rows > 0;
    }

    public async Task<bool> DeleteMovieAsync(int movieId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "DELETE FROM dbo.Movie WHERE MovieId = @MovieId";
        var rows = await connection.ExecuteAsync(sql, new { MovieId = movieId });
        return rows > 0;
    }
}
