using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Bookings;

public class ViewRecentBookingsUseCase : IViewRecentBookingsUseCase
{
    private readonly IBookingRepository _bookingRepo;

    public ViewRecentBookingsUseCase(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<IEnumerable<Booking>> ExecuteAsync(int count = 20)
    {
        return await _bookingRepo.GetRecentBookingsAsync(count);
    }
}
