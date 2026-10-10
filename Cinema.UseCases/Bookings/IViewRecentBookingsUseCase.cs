using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Bookings;

public interface IViewRecentBookingsUseCase
{
    Task<IEnumerable<Booking>> ExecuteAsync(int count = 20);
}
