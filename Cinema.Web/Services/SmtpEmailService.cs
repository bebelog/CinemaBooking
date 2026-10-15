using System.Net;
using System.Net.Mail;
using System.Text;
using Cinema.UseCases.PluginInterfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cinema.Web.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendTicketEmailAsync(
        string recipientEmail,
        string recipientName,
        string bookingReference,
        string movieTitle,
        DateTime showtimeStart,
        string auditoriumName,
        string screenType,
        IEnumerable<string> seatNames,
        decimal totalAmount,
        string paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            _logger.LogWarning("[SmtpEmailService] Recipient email is empty. Cannot send e-ticket.");
            return;
        }

        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPortStr = _config["EmailSettings:SmtpPort"];
        int smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var senderPassword = _config["EmailSettings:SenderPassword"];
        var senderName = _config["EmailSettings:SenderName"] ?? "Starlight Cinema Huế";
        var enableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;

        // If credentials are not yet configured in appsettings, simulate and log safely
        if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
        {
            _logger.LogInformation(
                "[SmtpEmailService] Email settings not configured (SenderEmail/SenderPassword is blank). Simulated ticket email dispatch to {Email} for BookingRef: {Ref}.",
                recipientEmail, bookingReference);
            return;
        }

        try
        {
            var seatListDisplay = string.Join(", ", seatNames);
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName, Encoding.UTF8),
                Subject = $"[Starlight Cinema Huế] Vé Điện Tử #{bookingReference} - {movieTitle}",
                Body = BuildHtmlBody(
                    recipientName,
                    bookingReference,
                    movieTitle,
                    showtimeStart,
                    auditoriumName,
                    screenType,
                    seatListDisplay,
                    totalAmount,
                    paymentMethod),
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };
            mailMessage.To.Add(new MailAddress(recipientEmail, recipientName, Encoding.UTF8));

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 12000
            };

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("[SmtpEmailService] Successfully sent ticket email to {Email} for {Ref}", recipientEmail, bookingReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmtpEmailService] Error dispatching email to {Email} for booking {Ref}", recipientEmail, bookingReference);
        }
    }

    private static string BuildHtmlBody(
        string recipientName,
        string bookingReference,
        string movieTitle,
        DateTime showtimeStart,
        string auditoriumName,
        string screenType,
        string seatListDisplay,
        decimal totalAmount,
        string paymentMethod)
    {
        string qrApiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=160x160&data={Uri.EscapeDataString(bookingReference)}";

        return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Vé Điện Tử - Starlight Cinema Huế</title>
</head>
<body style=""margin: 0; padding: 20px 0; background-color: #0b1120; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #334155;"">
    <div style=""max-width: 620px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.3);"">
        <!-- Cinema Banner Header -->
        <div style=""background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%); padding: 28px 24px; text-align: center; border-bottom: 4px solid #f59e0b;"">
            <h1 style=""margin: 0; color: #f59e0b; font-size: 24px; letter-spacing: 1.5px; text-transform: uppercase;"">⭐ STARLIGHT CINEMA HUẾ</h1>
            <p style=""margin: 6px 0 0 0; color: #94a3b8; font-size: 13px; letter-spacing: 1px;"">XÁC NHẬN VÉ XEM PHIM ĐIỆN TỬ (E-TICKET)</p>
        </div>

        <div style=""padding: 24px 28px;"">
            <p style=""font-size: 15px; line-height: 1.5; color: #334155; margin-top: 0;"">
                Xin chào <strong>{WebUtility.HtmlEncode(recipientName)}</strong>,<br>
                Cảm ơn bạn đã lựa chọn <strong>Starlight Cinema Huế</strong>. Giao dịch đặt vé của bạn đã hoàn tất thành công. Dưới đây là thông tin vé điện tử của bạn:
            </p>

            <!-- Booking Reference Highlight Box -->
            <div style=""background-color: #fef3c7; border: 2px dashed #f59e0b; border-radius: 10px; padding: 16px 20px; text-align: center; margin: 20px 0;"">
                <div style=""color: #92400e; font-size: 12px; font-weight: bold; text-transform: uppercase; letter-spacing: 1px;"">MÃ TRA CỨU ĐƠN VÉ (BOOKING CODE)</div>
                <div style=""color: #b45309; font-size: 26px; font-weight: 800; letter-spacing: 2.5px; margin-top: 4px; font-family: monospace;"">{WebUtility.HtmlEncode(bookingReference)}</div>
                <div style=""color: #78350f; font-size: 12px; margin-top: 4px;"">Vui lòng lưu giữ mã này để tra cứu hoặc xuất trình tại cổng soát vé.</div>
            </div>

            <!-- Ticket Details Table -->
            <table style=""width: 100%; border-collapse: collapse; margin-top: 16px; font-size: 14px;"">
                <tr style=""border-bottom: 1px solid #e2e8f0;"">
                    <td style=""padding: 10px 0; color: #64748b; width: 35%;"">🎬 Tên Phim:</td>
                    <td style=""padding: 10px 0; color: #0f172a; font-weight: 700; font-size: 15px;"">{WebUtility.HtmlEncode(movieTitle)}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #e2e8f0;"">
                    <td style=""padding: 10px 0; color: #64748b;"">🕒 Suất Chiếu:</td>
                    <td style=""padding: 10px 0; color: #0284c7; font-weight: 700;"">{showtimeStart:HH:mm} - {showtimeStart:dd/MM/yyyy}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #e2e8f0;"">
                    <td style=""padding: 10px 0; color: #64748b;"">🏛️ Phòng Chiếu:</td>
                    <td style=""padding: 10px 0; color: #0f172a; font-weight: 600;"">{WebUtility.HtmlEncode(auditoriumName)} <span style=""background: #e0f2fe; color: #0284c7; padding: 2px 8px; border-radius: 4px; font-size: 12px;"">{WebUtility.HtmlEncode(screenType)}</span></td>
                </tr>
                <tr style=""border-bottom: 1px solid #e2e8f0;"">
                    <td style=""padding: 10px 0; color: #64748b;"">💺 Vị Trí Ghế:</td>
                    <td style=""padding: 10px 0; color: #b45309; font-weight: 800; font-size: 16px;"">{WebUtility.HtmlEncode(seatListDisplay)}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #e2e8f0;"">
                    <td style=""padding: 10px 0; color: #64748b;"">💳 Thanh Toán:</td>
                    <td style=""padding: 10px 0; color: #334155; font-weight: 600;"">{WebUtility.HtmlEncode(paymentMethod)} (Đã xác nhận)</td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0; color: #64748b; font-size: 15px;"">💰 Tổng Tiền:</td>
                    <td style=""padding: 12px 0; color: #16a34a; font-weight: 800; font-size: 20px;"">{totalAmount:N0} VNĐ</td>
                </tr>
            </table>

            <!-- QR Code & Check-in instructions -->
            <div style=""background: #f8fafc; border-radius: 10px; padding: 18px; margin-top: 22px; text-align: center; border: 1px solid #e2e8f0;"">
                <div style=""font-size: 13px; font-weight: bold; color: #0f172a; margin-bottom: 12px;"">MÃ QR QUÉT TẠI CỬA SOÁT VÉ</div>
                <img src=""{qrApiUrl}"" alt=""QR Code {bookingReference}"" width=""140"" height=""140"" style=""display: inline-block; border-radius: 8px; border: 1px solid #cbd5e1;"" />
                <div style=""color: #64748b; font-size: 12px; margin-top: 10px; line-height: 1.5;"">
                    Quý khách có thể xuất trình mã QR này hoặc mã đơn vé trên điện thoại cho nhân viên soát vé trước giờ chiếu 15 phút.
                </div>
            </div>

            <!-- Public Lookup Guide -->
            <div style=""margin-top: 20px; padding: 12px 16px; background-color: #f0fdf4; border-left: 4px solid #22c55e; border-radius: 4px; font-size: 13px; color: #166534;"">
                <strong>💡 Mẹo tra cứu:</strong> Bạn có thể dùng mã <strong>{WebUtility.HtmlEncode(bookingReference)}</strong> để xem lại vé hoặc in lại bất kỳ lúc nào tại mục <em>Tra Cứu Vé</em> trên website của chúng tôi.
            </div>
        </div>

        <!-- Footer -->
        <div style=""background-color: #0f172a; padding: 20px 24px; text-align: center; font-size: 12px; color: #94a3b8; line-height: 1.6;"">
            <strong style=""color: #f59e0b;"">STARLIGHT CINEMA HUẾ</strong><br>
            📍 Tầng 4, TTTM Vincom Plaza Huế, 50A Hùng Vương, TP. Huế<br>
            📞 Hotline: <strong>1900 1234</strong> | ✉️ Email: <strong>hotro@starlightcinema.vn</strong><br>
            <span style=""font-size: 11px; color: #64748b; display: block; margin-top: 8px;"">Đây là email tự động từ hệ thống Starlight Cinema Huế. Vui lòng không trả lời trực tiếp email này.</span>
        </div>
    </div>
</body>
</html>";
    }
}
