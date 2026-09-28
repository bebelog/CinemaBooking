using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface IAuditoriumRepository
{
    Task<IEnumerable<Auditorium>> GetAllAuditoriumsAsync();
    Task<Auditorium?> GetAuditoriumByIdAsync(int auditoriumId);
}
