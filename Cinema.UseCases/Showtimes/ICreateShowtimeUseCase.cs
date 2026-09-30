using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Showtimes;

public interface ICreateShowtimeUseCase
{
    Task<int> ExecuteAsync(Showtime newShowtime);
}
