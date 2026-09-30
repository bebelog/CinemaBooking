using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Auditoriums;

public class ViewAuditoriumsUseCase : IViewAuditoriumsUseCase
{
    private readonly IAuditoriumRepository _repo;
    public ViewAuditoriumsUseCase(IAuditoriumRepository repo) => _repo = repo;
    public async Task<IEnumerable<Auditorium>> ExecuteAsync() => await _repo.GetAllAuditoriumsAsync();
}
