using System.Data;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Dapper;

namespace Cinema.DataStore.SQL.Dapper.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SeatRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Seat>> GetSeatsByAuditoriumIdAsync(int auditoriumId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT SeatId, AuditoriumId, RowCode, SeatNumber, SeatType, PriceModifier
            FROM dbo.Seat
            WHERE AuditoriumId = @AuditoriumId
            ORDER BY RowCode, SeatNumber";

        var result = await connection.QueryAsync<Seat>(sql, new { AuditoriumId = auditoriumId });
        return result.ToList();
    }

    public async Task<IEnumerable<int>> GetBookedSeatIdsByShowtimeIdAsync(int showtimeId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT SeatId FROM dbo.Ticket WHERE ShowtimeId = @ShowtimeId";
        var result = await connection.QueryAsync<int>(sql, new { ShowtimeId = showtimeId });
        return result.ToList();
    }
}
