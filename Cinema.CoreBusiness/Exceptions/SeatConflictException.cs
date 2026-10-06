namespace Cinema.CoreBusiness.Exceptions;

public class SeatConflictException : Exception
{
    public SeatConflictException(string message) : base(message) { }
    public SeatConflictException(int seatId, string seatName) 
        : base($"Ghế {seatName} (Mã #{seatId}) đã được đặt hoặc đang có người giữ. Vui lòng chọn ghế khác!") { }
}
