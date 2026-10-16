using Cinema.CoreBusiness.Models;
using Cinema.UseCases.Bookings;
using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Cinema.UseCases.PluginInterfaces.Services;
using Cinema.UseCases.PluginInterfaces.StateStore;
using Xunit;

namespace Cinema.UnitTests;

public class BookingEmailNotificationTests
{
    private class FakeBookingRepository : IBookingRepository
    {
        public Task<string> CreateBookingWithTicketsAsync(Booking booking, IEnumerable<Ticket> tickets)
        {
            return Task.FromResult(booking.BookingReference);
        }

        public Task<Booking?> GetBookingByReferenceAsync(string reference) => Task.FromResult<Booking?>(null);
        public Task<Booking?> GetBookingByIdAsync(int bookingId) => Task.FromResult<Booking?>(null);
        public Task<IEnumerable<Booking>> GetRecentBookingsAsync(int count = 20) => Task.FromResult(Enumerable.Empty<Booking>());
        public Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync() => Task.FromResult(Enumerable.Empty<OccupancyReportDto>()); public Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(int? userId, string? email, string? phone) => Task.FromResult(Enumerable.Empty<Booking>());
    }

    private class FakeSeatRepository : ISeatRepository
    {
        public Task<IEnumerable<Seat>> GetSeatsByAuditoriumIdAsync(int auditoriumId)
        {
            var seats = new List<Seat>
            {
                new() { SeatId = 1, AuditoriumId = 1, RowCode = "A", SeatNumber = 1, SeatType = "Standard", PriceModifier = 0 },
                new() { SeatId = 2, AuditoriumId = 1, RowCode = "A", SeatNumber = 2, SeatType = "Standard", PriceModifier = 0 },
                new() { SeatId = 3, AuditoriumId = 1, RowCode = "A", SeatNumber = 3, SeatType = "Standard", PriceModifier = 0 },
                new() { SeatId = 4, AuditoriumId = 1, RowCode = "A", SeatNumber = 4, SeatType = "Standard", PriceModifier = 0 }
            };
            return Task.FromResult<IEnumerable<Seat>>(seats);
        }

        public Task<IEnumerable<int>> GetBookedSeatIdsByShowtimeIdAsync(int showtimeId) => Task.FromResult(Enumerable.Empty<int>());
        public Task<Seat?> GetSeatByIdAsync(int seatId) => Task.FromResult<Seat?>(null);
    }

    private class FakeShowtimeRepository : IShowtimeRepository
    {
        public Task<Showtime?> GetShowtimeByIdAsync(int showtimeId)
        {
            return Task.FromResult<Showtime?>(new Showtime
            {
                ShowtimeId = showtimeId,
                MovieId = 1,
                AuditoriumId = 1,
                MovieTitle = "Lật Mặt 7",
                AuditoriumName = "Phòng Chiếu 1 - Huế",
                ScreenType = "2D Digital",
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now.AddHours(4),
                BasePrice = 90000
            });
        }

        public Task<IEnumerable<Showtime>> GetAllUpcomingShowtimesAsync() => Task.FromResult(Enumerable.Empty<Showtime>());
        public Task<IEnumerable<Showtime>> GetShowtimesByDateAsync(DateTime date) => Task.FromResult(Enumerable.Empty<Showtime>());
        public Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId) => Task.FromResult(Enumerable.Empty<Showtime>());
        public Task<IEnumerable<Showtime>> GetShowtimesByAuditoriumAndDateRangeAsync(int auditoriumId, DateTime start, DateTime end) => Task.FromResult(Enumerable.Empty<Showtime>());
        public Task<int> AddShowtimeAsync(Showtime showtime) => Task.FromResult(1);
        public Task<bool> DeleteShowtimeAsync(int showtimeId) => Task.FromResult(true);
    }

    private class FakeSeatHoldingStore : ISeatHoldingStateStore
    {
#pragma warning disable CS0067
        public event Action<int>? OnSeatsChanged;
#pragma warning restore CS0067
        public bool TryHoldSeats(int showtimeId, IEnumerable<int> seatIds, string sessionId, TimeSpan duration) => true;
        public void ReleaseSeats(int showtimeId, IEnumerable<int> seatIds, string sessionId) { }
        public IEnumerable<int> GetHeldSeatIds(int showtimeId, string excludeSessionId = "") => Enumerable.Empty<int>();
        public IEnumerable<int> GetMyHeldSeatIds(int showtimeId, string sessionId) => Enumerable.Empty<int>();
        public DateTime? GetHoldExpiryTime(int showtimeId, string sessionId) => DateTime.Now.AddMinutes(5);
        public void CleanExpiredHolds() { }
        public void ReleaseAllBySession(string sessionId) { }
        public void ClearAllHolds() { }
    }

    private class FakeEmailService : IEmailService
    {
        public bool WasCalled { get; private set; }
        public string? SentToEmail { get; private set; }
        public string? SentBookingRef { get; private set; }
        public bool ShouldThrow { get; set; }

        public Task SendTicketEmailAsync(
            string recipientEmail,
            string recipientName,
            string bookingReference,
            string movieTitle,
            DateTime showtimeStart,
            string auditoriumName,
            string screenType,
            IEnumerable<string> seatNames,
            decimal totalAmount,
            string paymentMethod)
        {
            if (ShouldThrow)
                throw new InvalidOperationException("SMTP connection timed out.");

            WasCalled = true;
            SentToEmail = recipientEmail;
            SentBookingRef = bookingReference;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidBookingAndEmail_DispatchesTicketEmail()
    {
        // Arrange
        var fakeEmail = new FakeEmailService();
        var useCase = new CreateBookingUseCase(
            new FakeBookingRepository(),
            new FakeSeatRepository(),
            new FakeShowtimeRepository(),
            new FakeSeatHoldingStore(),
            fakeEmail);

        var booking = new Booking
        {
            ShowtimeId = 1,
            CustomerName = "Nguyễn Văn Mẫn",
            CustomerEmail = "man.nguyen@gmail.com",
            CustomerPhone = "0905123456",
            PaymentMethod = "VNPAY"
        };

        // Act: Đặt 2 ghế A1, A2 (còn A3, A4 không bị đơn lẻ)
        string refCode = await useCase.ExecuteAsync(booking, new[] { 1, 2 }, "session-123");

        // Đợi một khoảng ngắn cho Task.Run hoàn tất
        await Task.Delay(250);

        // Assert
        Assert.NotNull(refCode);
        Assert.StartsWith("BK-", refCode);
        Assert.True(fakeEmail.WasCalled);
        Assert.Equal("man.nguyen@gmail.com", fakeEmail.SentToEmail);
        Assert.Equal(refCode, fakeEmail.SentBookingRef);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerEmailIsEmpty_DoesNotDispatchEmail()
    {
        // Arrange
        var fakeEmail = new FakeEmailService();
        var useCase = new CreateBookingUseCase(
            new FakeBookingRepository(),
            new FakeSeatRepository(),
            new FakeShowtimeRepository(),
            new FakeSeatHoldingStore(),
            fakeEmail);

        var booking = new Booking
        {
            ShowtimeId = 1,
            CustomerName = "Khách Vãng Lai",
            CustomerEmail = "", // Không có email
            CustomerPhone = "0905999999",
            PaymentMethod = "Tiền Mặt"
        };

        // Act
        string refCode = await useCase.ExecuteAsync(booking, new[] { 1, 2 }, "session-456");
        await Task.Delay(250);

        // Assert
        Assert.NotNull(refCode);
        Assert.False(fakeEmail.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailServiceThrows_BookingStillSucceedsResiliently()
    {
        // Arrange
        var fakeEmail = new FakeEmailService { ShouldThrow = true };
        var useCase = new CreateBookingUseCase(
            new FakeBookingRepository(),
            new FakeSeatRepository(),
            new FakeShowtimeRepository(),
            new FakeSeatHoldingStore(),
            fakeEmail);

        var booking = new Booking
        {
            ShowtimeId = 1,
            CustomerName = "Trần Thị Cúc",
            CustomerEmail = "cuc.tran@gmail.com",
            CustomerPhone = "0905888888",
            PaymentMethod = "MOMO"
        };

        // Act: Đặt vé phải thành công 100%, không bị crash dù email lỗi
        string refCode = await useCase.ExecuteAsync(booking, new[] { 1, 2 }, "session-789");
        await Task.Delay(250);

        // Assert
        Assert.NotNull(refCode);
        Assert.StartsWith("BK-", refCode);
    }
}
