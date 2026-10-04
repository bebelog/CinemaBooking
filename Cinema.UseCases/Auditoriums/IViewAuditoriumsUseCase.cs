using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Auditoriums;

public interface IViewAuditoriumsUseCase
{
    Task<IEnumerable<Auditorium>> ExecuteAsync();
}
