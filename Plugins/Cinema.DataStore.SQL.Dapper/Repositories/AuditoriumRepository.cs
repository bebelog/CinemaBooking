using System.Data;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Dapper;

namespace Cinema.DataStore.SQL.Dapper.Repositories;

public class AuditoriumRepository : IAuditoriumRepository
{
    private readonly ISqlConnectionFactory _factory;

    public AuditoriumRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Auditorium>> GetAllAuditoriumsAsync()
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT AuditoriumId, Name, TotalSeats, ScreenType FROM dbo.Auditorium ORDER BY AuditoriumId";
        var result = await connection.QueryAsync<Auditorium>(sql);
        return result.ToList();
    }

    public async Task<Auditorium?> GetAuditoriumByIdAsync(int auditoriumId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT AuditoriumId, Name, TotalSeats, ScreenType FROM dbo.Auditorium WHERE AuditoriumId = @AuditoriumId";
        return await connection.QueryFirstOrDefaultAsync<Auditorium>(sql, new { AuditoriumId = auditoriumId });
    }
}
