using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Reports;

public class ViewOccupancyReportUseCase : IViewOccupancyReportUseCase
{
    private readonly IBookingRepository _bookingRepo;

    public ViewOccupancyReportUseCase(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<IEnumerable<OccupancyReportDto>> ExecuteAsync()
    {
        return await _bookingRepo.GetOccupancyReportAsync();
    }
}
