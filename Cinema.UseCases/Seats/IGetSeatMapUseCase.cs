using Cinema.UseCases.DTOs;

namespace Cinema.UseCases.Seats;

public interface IGetSeatMapUseCase
{
    Task<IEnumerable<SeatDto>> ExecuteAsync(int showtimeId, string currentSessionId);
}
