using Cinema.UseCases.DTOs;

namespace Cinema.UseCases.Reports;

public interface IViewOccupancyReportUseCase
{
    Task<IEnumerable<OccupancyReportDto>> ExecuteAsync();
}
