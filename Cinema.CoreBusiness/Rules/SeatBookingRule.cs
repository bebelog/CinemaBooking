using Cinema.CoreBusiness.Exceptions;

namespace Cinema.CoreBusiness.Rules;

/// <summary>
/// LUẬT 1: Tranh chấp ghế (Seat Conflict Rule)
/// - Một ghế trong một suất chiếu chỉ được bán DUY NHẤT một lần.
/// - Ngăn chặn race condition khi 2 khách hàng cùng chọn hoặc bấm đặt vé cùng lúc.
/// </summary>
public static class SeatBookingRule
{
    public static void ValidateSeatsAvailability(
        IEnumerable<int> requestedSeatIds, 
        IEnumerable<int> alreadyBookedSeatIds,
        IEnumerable<int>? currentlyHeldByOthersSeatIds = null)
    {
        var bookedSet = new HashSet<int>(alreadyBookedSeatIds);
        var heldSet = currentlyHeldByOthersSeatIds != null 
            ? new HashSet<int>(currentlyHeldByOthersSeatIds) 
            : new HashSet<int>();

        foreach (var seatId in requestedSeatIds)
        {
            if (bookedSet.Contains(seatId))
            {
                throw new SeatConflictException($"Ghế mã #{seatId} đã được mua vé thành công bởi khách hàng khác. Vui lòng chọn ghế khác!");
            }

            if (heldSet.Contains(seatId))
            {
                throw new SeatConflictException($"Ghế mã #{seatId} đang được giữ chỗ trong phiên đặt vé của người khác. Vui lòng thử lại sau!");
            }
        }
    }
}
