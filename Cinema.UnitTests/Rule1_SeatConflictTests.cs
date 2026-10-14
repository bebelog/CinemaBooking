using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Models;
using Cinema.CoreBusiness.Rules;
using Xunit;

namespace Cinema.UnitTests;

/// <summary>
/// Kiểm thử LUẬT 1: Tranh chấp & Quy tắc đặt ghế (Seat Booking & Conflict Rule)
/// Bao gồm:
/// - Chống bán trùng ghế đã mua
/// - Chống tranh chấp ghế đang bị giữ chỗ
/// - Giới hạn số lượng ghế (1 - 8 ghế)
/// - Quy tắc không để lại ghế trống đơn lẻ (Single Orphan Seat Rule)
/// </summary>
public class Rule1_SeatConflictTests
{
    // --- TẦNG 1: KIỂM THỬ TÍNH KHẢ DỤNG & TRANH CHẤP GHẾ ---

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

        // Act & Assert: Phải pass không có ngoại lệ
        SeatBookingRule.ValidateSeatsAvailability(requestedSeats, bookedSeats, heldByOthers);
    }

    // --- TẦNG 2: KIỂM THỬ GIỚI HẠN SỐ LƯỢNG GHẾ ĐẶT (1 - 8 GHẾ) ---

    [Fact]
    public void ValidateSeatCount_WhenZeroSeats_ThrowsSeatConflictException()
    {
        // Arrange
        var emptySeats = new List<int>();

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateSeatCount(emptySeats));

        Assert.Contains("chọn ít nhất 1 ghế", ex.Message);
    }

    [Fact]
    public void ValidateSeatCount_WhenExceedsMax8Seats_ThrowsSeatConflictException()
    {
        // Arrange: Chọn 9 ghế (vượt mức tối đa 8 ghế)
        var nineSeats = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateSeatCount(nineSeats));

        Assert.Contains("tối đa 8 ghế", ex.Message);
    }

    [Fact]
    public void ValidateSeatCount_WhenBetween1And8Seats_DoesNotThrow()
    {
        // Arrange: Chọn 4 ghế hợp lệ
        var validSeats = new List<int> { 1, 2, 3, 4 };

        // Act & Assert: Pass không có lỗi
        SeatBookingRule.ValidateSeatCount(validSeats);
    }

    // --- TẦNG 3: KIỂM THỬ LUẬT CHỐNG GHẾ TRỐNG ĐƠN LẺ (SINGLE ORPHAN SEAT) ---

    private List<Seat> CreateSampleRowA()
    {
        // Tạo hàng ghế A gồm 8 ghế: A1 đến A8
        return Enumerable.Range(1, 8).Select(i => new Seat
        {
            SeatId = i,
            AuditoriumId = 1,
            RowCode = "A",
            SeatNumber = i
        }).ToList();
    }

    [Fact]
    public void ValidateNoSingleOrphanSeat_WhenLeavesSingleGapAtAisle_ThrowsSeatConflictException()
    {
        // Arrange: Hàng A ban đầu trống cả 8 ghế. Khách chọn A2, A3 (để lại đúng ghế A1 đơn lẻ cạnh lối đi)
        var allSeats = CreateSampleRowA();
        var selectedSeats = new List<int> { 2, 3 }; // Chọn A2, A3 -> Bỏ lại A1 đơn lẻ
        var unavailableSeats = new List<int>();

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateNoSingleOrphanSeat(allSeats, selectedSeats, unavailableSeats));

        Assert.Contains("ghế trống đơn lẻ", ex.Message);
        Assert.Contains("A1", ex.Message);
    }

    [Fact]
    public void ValidateNoSingleOrphanSeat_WhenLeavesSingleGapInMiddle_ThrowsSeatConflictException()
    {
        // Arrange: Ghế A1 đã có người mua. Khách chọn A3, A4 (để lại đúng ghế A2 bị kẹp giữa A1 và A3)
        var allSeats = CreateSampleRowA();
        var selectedSeats = new List<int> { 3, 4 };
        var unavailableSeats = new List<int> { 1 }; // A1 đã mua -> A2 bị kẹp lẻ loi

        // Act & Assert
        var ex = Assert.Throws<SeatConflictException>(() =>
            SeatBookingRule.ValidateNoSingleOrphanSeat(allSeats, selectedSeats, unavailableSeats));

        Assert.Contains("ghế trống đơn lẻ", ex.Message);
        Assert.Contains("A2", ex.Message);
    }

    [Fact]
    public void ValidateNoSingleOrphanSeat_WhenLeavesPairOfEmptySeats_DoesNotThrow()
    {
        // Arrange: Khách chọn A3, A4 (để lại A1 và A2 là 2 ghế trống liền kề -> Hợp lệ cho cặp đôi)
        var allSeats = CreateSampleRowA();
        var selectedSeats = new List<int> { 3, 4 };
        var unavailableSeats = new List<int>();

        // Act & Assert: Phải pass hợp lệ
        SeatBookingRule.ValidateNoSingleOrphanSeat(allSeats, selectedSeats, unavailableSeats);
    }

    [Fact]
    public void ValidateNoSingleOrphanSeat_WhenSelectingFromFirstSeat_DoesNotThrow()
    {
        // Arrange: Khách chọn A1, A2, A3 liền kề từ đầu hàng -> Hợp lệ
        var allSeats = CreateSampleRowA();
        var selectedSeats = new List<int> { 1, 2, 3 };
        var unavailableSeats = new List<int>();

        // Act & Assert: Phải pass hợp lệ
        SeatBookingRule.ValidateNoSingleOrphanSeat(allSeats, selectedSeats, unavailableSeats);
    }
}
