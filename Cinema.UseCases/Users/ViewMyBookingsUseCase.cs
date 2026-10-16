using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Users;

public interface IViewMyBookingsUseCase
{
    Task<IEnumerable<Booking>> ExecuteAsync(int? userId, string? email, string? phone);
}

public class ViewMyBookingsUseCase : IViewMyBookingsUseCase
{
    private readonly IBookingRepository _bookingRepo;

    public ViewMyBookingsUseCase(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<IEnumerable<Booking>> ExecuteAsync(int? userId, string? email, string? phone)
    {
        return await _bookingRepo.GetBookingsByCustomerAsync(userId, email, phone);
    }
}
