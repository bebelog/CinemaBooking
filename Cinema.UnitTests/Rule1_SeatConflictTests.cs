using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Rules;
using Xunit;

namespace Cinema.UnitTests;

/// <summary>
/// Kiểm thử LUẬT 1: Tranh chấp ghế (Seat Conflict)
/// Đảm bảo ghế đã mua hoặc đang giữ không thể bị đặt trùng
/// </summary>
public class Rule1_SeatConflictTests
{
    [Fact]
    public void ValidateSeats_WhenSeatAlreadyBooked_ThrowsSeatConflictException()
    {
        // Arrange
        var requestedSeats = new List<int> { 1, 2, 3 };
        var bookedSeats = new List<int> { 2 }; // Ghế 2 đã được mua
        var heldSeats = new List<int> { 5 };

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateSeatsAvailability(requestedSeats, bookedSeats, heldSeats));

        Assert.Contains("đã được mua vé thành công", ex.Message);
    }

    [Fact]
    public void ValidateSeats_WhenSeatHeldByOther_ThrowsSeatConflictException()
    {
        // Arrange
        var requestedSeats = new List<int> { 10, 11 };
        var bookedSeats = new List<int> { 1, 2 };
        var heldByOthers = new List<int> { 11 }; // Ghế 11 đang bị giữ bởi phiên khác

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateSeatsAvailability(requestedSeats, bookedSeats, heldByOthers));

        Assert.Contains("đang được giữ chỗ", ex.Message);
    }

    [Fact]
    public void ValidateSeats_WhenAllSeatsAvailable_DoesNotThrow()
    {
        // Arrange
        var requestedSeats = new List<int> { 10, 11 };
        var bookedSeats = new List<int> { 1, 2 };
        var heldByOthers = new List<int> { 3, 4 };

        // Act & Assert (Should pass cleanly without exceptions)
        SeatBookingRule.ValidateSeatsAvailability(requestedSeats, bookedSeats, heldByOthers);
    }
}
