using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Cinema.UseCases.PluginInterfaces.StateStore;

namespace Cinema.UseCases.Seats;

public class GetSeatMapUseCase : IGetSeatMapUseCase
{
    private readonly ISeatRepository _seatRepo;
    private readonly IShowtimeRepository _showtimeRepo;
    private readonly ISeatHoldingStateStore _holdingStore;

    public GetSeatMapUseCase(
        ISeatRepository seatRepo, 
        IShowtimeRepository showtimeRepo, 
        ISeatHoldingStateStore holdingStore)
    {
        _seatRepo = seatRepo;
        _showtimeRepo = showtimeRepo;
        _holdingStore = holdingStore;
    }

    public async Task<IEnumerable<SeatDto>> ExecuteAsync(int showtimeId, string currentSessionId)
    {
        var showtime = await _showtimeRepo.GetShowtimeByIdAsync(showtimeId);
        if (showtime == null) 
            throw new InvalidOperationException("Suất chiếu không tồn tại.");

        var seats = await _seatRepo.GetSeatsByAuditoriumIdAsync(showtime.AuditoriumId);
        var bookedSeatIds = (await _seatRepo.GetBookedSeatIdsByShowtimeIdAsync(showtimeId)).ToHashSet();
        var heldSeatIdsByOthers = _holdingStore.GetHeldSeatIds(showtimeId, excludeSessionId: currentSessionId).ToHashSet();
        var myHeldSeatIds = _holdingStore.GetMyHeldSeatIds(showtimeId, currentSessionId).ToHashSet();

        return seats.Select(s => new SeatDto
        {
            SeatId = s.SeatId,
            AuditoriumId = s.AuditoriumId,
            RowCode = s.RowCode,
            SeatNumber = s.SeatNumber,
            SeatType = s.SeatType,
            PriceModifier = s.PriceModifier,
            FinalPrice = showtime.BasePrice + s.PriceModifier,
            IsBooked = bookedSeatIds.Contains(s.SeatId),
            IsHeldByOther = heldSeatIdsByOthers.Contains(s.SeatId),
            IsHeldByMe = myHeldSeatIds.Contains(s.SeatId)
        });
    }
}
