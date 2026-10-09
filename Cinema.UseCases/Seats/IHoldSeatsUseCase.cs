namespace Cinema.UseCases.Seats;

public interface IHoldSeatsUseCase
{
    bool TryHold(int showtimeId, IEnumerable<int> seatIds, string sessionId, TimeSpan duration);
    void Release(int showtimeId, IEnumerable<int> seatIds, string sessionId);
    DateTime? GetHoldExpiry(int showtimeId, string sessionId);
}