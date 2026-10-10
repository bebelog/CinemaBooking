using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Bookings;

public interface ICreateBookingUseCase
{
    Task<string> ExecuteAsync(Booking booking, IEnumerable<int> selectedSeatIds, string sessionId);
}
