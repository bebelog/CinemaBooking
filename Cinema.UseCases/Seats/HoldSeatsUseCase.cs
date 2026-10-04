using Cinema.UseCases.PluginInterfaces.StateStore;

namespace Cinema.UseCases.Seats;

public class HoldSeatsUseCase : IHoldSeatsUseCase
{
    private readonly ISeatHoldingStateStore _holdingStore;

    public HoldSeatsUseCase(ISeatHoldingStateStore holdingStore)
    {
        _holdingStore = holdingStore;
    }

    public bool TryHold(int showtimeId, IEnumerable<int> seatIds, string sessionId, TimeSpan duration)
    {
        return _holdingStore.TryHoldSeats(showtimeId, seatIds, sessionId, duration);
    }

    public void Release(int showtimeId, IEnumerable<int> seatIds, string sessionId)
    {
        _holdingStore.ReleaseSeats(showtimeId, seatIds, sessionId);
    }

    public DateTime? GetHoldExpiry(int showtimeId, string sessionId)
    {
        return _holdingStore.GetHoldExpiryTime(showtimeId, sessionId);
    }
}