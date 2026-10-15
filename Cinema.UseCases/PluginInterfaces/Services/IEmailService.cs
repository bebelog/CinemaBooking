namespace Cinema.UseCases.PluginInterfaces.Services;

/// <summary>
/// Dịch vụ gửi email thông báo vé điện tử (E-Ticket) cho khách hàng
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email xác nhận đặt vé thành công kèm mã tra cứu và chi tiết vé
    /// </summary>
    Task SendTicketEmailAsync(
        string recipientEmail,
        string recipientName,
        string bookingReference,
        string movieTitle,
        DateTime showtimeStart,
        string auditoriumName,
        string screenType,
        IEnumerable<string> seatNames,
        decimal totalAmount,
        string paymentMethod);
}
