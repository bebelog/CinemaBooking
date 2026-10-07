using System.Data;
using Microsoft.Data.SqlClient;
using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Dapper;

namespace Cinema.DataStore.SQL.Dapper.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ISqlConnectionFactory _factory;

    public BookingRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> CreateBookingWithTicketsAsync(Booking booking, IEnumerable<Ticket> tickets)
    {
        using var connection = _factory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            const string insertBookingSql = @"
                INSERT INTO dbo.Booking (BookingReference, ShowtimeId, CustomerName, CustomerEmail, CustomerPhone, TotalAmount, BookingTime, Status, PaymentMethod)
                VALUES (@BookingReference, @ShowtimeId, @CustomerName, @CustomerEmail, @CustomerPhone, @TotalAmount, @BookingTime, @Status, @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var bookingId = await connection.ExecuteScalarAsync<int>(insertBookingSql, booking, transaction);
            booking.BookingId = bookingId;

            const string insertTicketSql = @"
                INSERT INTO dbo.Ticket (BookingId, ShowtimeId, SeatId, TicketPrice, TicketCode)
                VALUES (@BookingId, @ShowtimeId, @SeatId, @TicketPrice, @TicketCode);";

            foreach (var ticket in tickets)
            {
                ticket.BookingId = bookingId;
                await connection.ExecuteAsync(insertTicketSql, ticket, transaction);
            }

            transaction.Commit();
            return booking.BookingReference;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            transaction.Rollback();
            throw new SeatConflictException("Tranh chấp ghế xảy ra: Một trong những ghế bạn chọn vừa được khách hàng khác thanh toán thành công. Vui lòng chọn ghế khác!");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Booking?> GetBookingByReferenceAsync(string reference)
    {
        using var connection = _factory.CreateConnection();
        const string bookingSql = @"
            SELECT BookingId, BookingReference, ShowtimeId, CustomerName, CustomerEmail, CustomerPhone, TotalAmount, BookingTime, Status, PaymentMethod
            FROM dbo.Booking
            WHERE BookingReference = @Reference";

        var booking = await connection.QueryFirstOrDefaultAsync<Booking>(bookingSql, new { Reference = reference });
        if (booking == null) return null;

        const string ticketsSql = @"
            SELECT t.TicketId, t.BookingId, t.ShowtimeId, t.SeatId, t.TicketPrice, t.TicketCode,
                   s.RowCode + CAST(s.SeatNumber AS VARCHAR(5)) AS SeatDisplayName,
                   s.SeatType
            FROM dbo.Ticket t
            INNER JOIN dbo.Seat s ON t.SeatId = s.SeatId
            WHERE t.BookingId = @BookingId";

        var tickets = await connection.QueryAsync<Ticket>(ticketsSql, new { BookingId = booking.BookingId });
        booking.Tickets = tickets.ToList();

        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        using var connection = _factory.CreateConnection();
        const string bookingSql = @"
            SELECT BookingId, BookingReference, ShowtimeId, CustomerName, CustomerEmail, CustomerPhone, TotalAmount, BookingTime, Status, PaymentMethod
            FROM dbo.Booking
            WHERE BookingId = @BookingId";

        var booking = await connection.QueryFirstOrDefaultAsync<Booking>(bookingSql, new { BookingId = bookingId });
        if (booking == null) return null;

        const string ticketsSql = @"
            SELECT t.TicketId, t.BookingId, t.ShowtimeId, t.SeatId, t.TicketPrice, t.TicketCode,
                   s.RowCode + CAST(s.SeatNumber AS VARCHAR(5)) AS SeatDisplayName,
                   s.SeatType
            FROM dbo.Ticket t
            INNER JOIN dbo.Seat s ON t.SeatId = s.SeatId
            WHERE t.BookingId = @BookingId";

        var tickets = await connection.QueryAsync<Ticket>(ticketsSql, new { BookingId = booking.BookingId });
        booking.Tickets = tickets.ToList();

        return booking;
    }

    public async Task<IEnumerable<Booking>> GetRecentBookingsAsync(int count = 20)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT TOP (@Count) BookingId, BookingReference, ShowtimeId, CustomerName, CustomerEmail, CustomerPhone, TotalAmount, BookingTime, Status, PaymentMethod
            FROM dbo.Booking
            ORDER BY BookingTime DESC";

        var result = await connection.QueryAsync<Booking>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync()
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT s.ShowtimeId, m.Title AS MovieTitle, a.Name AS AuditoriumName, s.StartTime, a.TotalSeats,
                   COUNT(t.TicketId) AS BookedSeats,
                   ISNULL(SUM(t.TicketPrice), 0) AS TotalRevenue
            FROM dbo.Showtime s
            INNER JOIN dbo.Movie m ON s.MovieId = m.MovieId
            INNER JOIN dbo.Auditorium a ON s.AuditoriumId = a.AuditoriumId
            LEFT JOIN dbo.Ticket t ON s.ShowtimeId = t.ShowtimeId
            GROUP BY s.ShowtimeId, m.Title, a.Name, s.StartTime, a.TotalSeats
            ORDER BY s.StartTime DESC";

        var result = await connection.QueryAsync<OccupancyReportDto>(sql);
        return result.ToList();
    }
}
