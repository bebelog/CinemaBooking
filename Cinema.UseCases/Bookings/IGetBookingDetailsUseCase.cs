using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.Bookings;

public interface IGetBookingDetailsUseCase
{
    Task<Booking?> ExecuteByReferenceAsync(string reference);
    Task<Booking?> ExecuteByIdAsync(int bookingId);
}
