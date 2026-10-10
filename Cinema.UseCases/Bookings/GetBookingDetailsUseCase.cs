using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Bookings;

public class GetBookingDetailsUseCase : IGetBookingDetailsUseCase
{
    private readonly IBookingRepository _bookingRepo;

    public GetBookingDetailsUseCase(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<Booking?> ExecuteByReferenceAsync(string reference)
    {
        return await _bookingRepo.GetBookingByReferenceAsync(reference);
    }

    public async Task<Booking?> ExecuteByIdAsync(int bookingId)
    {
        return await _bookingRepo.GetBookingByIdAsync(bookingId);
    }
}
