using Cinema.CoreBusiness.Models;
using Cinema.CoreBusiness.Rules;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Cinema.UseCases.PluginInterfaces.StateStore;

namespace Cinema.UseCases.Bookings;

/// <summary>
/// Đặt vé xem phim - Thực hiện LUẬT 1 (Tranh chấp ghế) và tạo Booking/Ticket theo Header-Line
/// </summary>
public class CreateBookingUseCase : ICreateBookingUseCase
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ISeatRepository _seatRepo;
    private readonly IShowtimeRepository _showtimeRepo;
    private readonly ISeatHoldingStateStore _holdingStore;

    public CreateBookingUseCase(
        IBookingRepository bookingRepo,
        ISeatRepository seatRepo,
        IShowtimeRepository showtimeRepo,
        ISeatHoldingStateStore holdingStore)
    {
        _bookingRepo = bookingRepo;
        _seatRepo = seatRepo;
        _showtimeRepo = showtimeRepo;
        _holdingStore = holdingStore;
    }

    public async Task<string> ExecuteAsync(Booking booking, IEnumerable<int> selectedSeatIds, string sessionId)
    {
        var seatIdsList = selectedSeatIds.Distinct().ToList();
        if (!seatIdsList.Any())
            throw new ArgumentException("Vui lòng chọn ít nhất một ghế để tiến hành thanh toán!");

        // 1. Kiểm tra sự tồn tại của suất chiếu
        var showtime = await _showtimeRepo.GetShowtimeByIdAsync(booking.ShowtimeId);
        if (showtime == null)
            throw new InvalidOperationException("Suất chiếu không tồn tại.");

        // 2. Áp dụng LUẬT 1: Tranh chấp ghế (SeatBookingRule)
        var bookedSeats = await _seatRepo.GetBookedSeatIdsByShowtimeIdAsync(booking.ShowtimeId);
        var heldByOthers = _holdingStore.GetHeldSeatIds(booking.ShowtimeId, excludeSessionId: sessionId);

        SeatBookingRule.ValidateSeatsAvailability(seatIdsList, bookedSeats, heldByOthers);

        // 3. Chuẩn bị line items (Ticket) và tính tổng tiền
        var allAuditoriumSeats = (await _seatRepo.GetSeatsByAuditoriumIdAsync(showtime.AuditoriumId))
            .ToDictionary(s => s.SeatId);

        var tickets = new List<Ticket>();
        decimal totalAmount = 0;
        string datePrefix = DateTime.Now.ToString("yyyyMMdd");
        string randomSuffix = Guid.NewGuid().ToString("N")[..6].ToUpper();
        booking.BookingReference = $"BK-{datePrefix}-{randomSuffix}";

        foreach (var seatId in seatIdsList)
        {
            if (!allAuditoriumSeats.TryGetValue(seatId, out var seat))
                throw new InvalidOperationException($"Ghế ID {seatId} không hợp lệ trong phòng chiếu này.");

            decimal ticketPrice = showtime.BasePrice + seat.PriceModifier;
            totalAmount += ticketPrice;

            tickets.Add(new Ticket
            {
                ShowtimeId = booking.ShowtimeId,
                SeatId = seatId,
                TicketPrice = ticketPrice,
                TicketCode = $"TK-{booking.ShowtimeId}-{seat.SeatDisplayName}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}"
            });
        }

        booking.TotalAmount = totalAmount;

        // 4. Lưu Booking & Tickets vào CSDL trong một Transaction
        string createdReference = await _bookingRepo.CreateBookingWithTicketsAsync(booking, tickets);

        // 5. Giải phóng trạng thái giữ ghế
        _holdingStore.ReleaseSeats(booking.ShowtimeId, seatIdsList, sessionId);

        return createdReference;
    }
}
