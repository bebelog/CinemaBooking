namespace Cinema.UseCases.Showtimes;

public interface IDeleteShowtimeUseCase
{
    Task<bool> ExecuteAsync(int showtimeId);
}
