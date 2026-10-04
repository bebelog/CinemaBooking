using Cinema.CoreBusiness.Models;
using Cinema.UseCases.DTOs;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface IBookingRepository
{
    Task<Booking?> GetBookingByReferenceAsync(string reference);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<string> CreateBookingWithTicketsAsync(Booking booking, IEnumerable<Ticket> tickets);
    Task<IEnumerable<Booking>> GetRecentBookingsAsync(int count = 20);
    Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync();
}
