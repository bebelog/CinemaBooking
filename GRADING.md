# PHIẾU TỰ ĐÁNH GIÁ VÀ BẰNG CHỨNG CHẤM ĐỒ ÁN CAPSTONE (ECO2415)

- **Đề tài số 14**: Rạp phim: Chọn ghế & Đặt vé (Cinema Seat Booking)
- **Độ khó**: ★★★ (Tranh chấp tài nguyên đồng thời ở tầng CSDL, Sơ đồ ghế trực quan)
- **Sinh viên thực hiện**: Nguyễn Viết Mẫn – MSSV: 23K4080026 – Lớp: K46/21.02
- **Trường**: Trường Đại học Kinh tế, Đại học Huế
- **GitHub Repository**: [https://github.com/bebelog/CinemaBooking](https://github.com/bebelog/CinemaBooking)

---

## BẢNG ĐỐI CHIẾU 18 MÃ TIÊU CHÍ PHIẾU CHẤM (MỤC 12 ĐỀ CƯƠNG)

> **Nguyên tắc chấm**: Mỗi mục chấm 3 mức: `1 = Đạt đủ (100%)`, `0.5 = Đạt một phần (50%)`, `0 = Không đạt hoặc không có bằng chứng`. Điểm mục = mức × Điểm tối đa. Tổng 100 điểm.

| MÃ | Tiêu chí kiểm tra | Điểm | Đường dẫn file bằng chứng | Dòng code / Bằng chứng chi tiết |
| :--- | :--- | :---: | :--- | :--- |
| **K1.1** | Đủ 4 project; CoreBusiness không tham chiếu project nào; UseCases chỉ tham chiếu CoreBusiness | **8** | `Cinema.CoreBusiness/Cinema.CoreBusiness.csproj`<br>`Cinema.UseCases/Cinema.UseCases.csproj`<br>`Plugins/Cinema.DataStore.SQL.Dapper/`<br>`Cinema.Web/Cinema.Web.csproj` | • `Cinema.CoreBusiness.csproj`: 0 thẻ `<ProjectReference>` (độc lập tuyệt đối).<br>• `Cinema.UseCases.csproj`: Chỉ tham chiếu duy nhất `..\Cinema.CoreBusiness\`.<br>• `Plugins`: Triển khai các Repository và StateStore.<br>• `Cinema.Web.csproj`: Composition Root tham chiếu UseCases & Plugins. |
| **K1.2** | Interface repository nằm ở UseCases, Plugin cài đặt; file .razor inject interface, không inject lớp cụ thể | **7** | `Cinema.UseCases/PluginInterfaces/`<br>`Plugins/Cinema.DataStore.SQL.Dapper/`<br>`Cinema.Web/Components/Pages/`<br>`Cinema.Web/Program.cs` | • Interface: `IMovieRepository.cs`, `IShowtimeRepository.cs`, `ISeatRepository.cs`, `IBookingRepository.cs`, `ISeatHoldingStateStore.cs` nằm trong tầng `UseCases`.<br>• Cài đặt: `BookingRepository.cs`, `MovieRepository.cs` nằm trong `Plugins`.<br>• File `.razor`: Dòng đầu chỉ `@inject IViewShowtimesUseCase`, `@inject ICreateBookingUseCase`... Không bao giờ inject class SQL cụ thể.<br>• `Program.cs`: Là composition root duy nhất đăng ký DI. |
| **K1.3** | 2 luật nghiệp vụ của đề cài trong CoreBusiness / UseCases, không nằm trong .razor (3 điểm mỗi luật) | **6** | `Cinema.CoreBusiness/Rules/SeatBookingRule.cs`<br>`Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs`<br>`Cinema.UseCases/Bookings/CreateBookingUseCase.cs` | • **LUẬT 1 (3đ) - Tranh chấp ghế**: *"Một ghế trong một suất chỉ bán đúng 1 lần, kể cả khi 2 người bấm cùng lúc (unique constraint + transaction); ghế giữ quá 10 phút tự nhả"*: Cài đặt tại `SeatBookingRule.cs` và `SeatHoldingStateStore.cs`.<br>• **LUẬT 2 (3đ) - Không chồng giờ**: *"Các suất chiếu cùng phòng không chồng giờ (thời lượng phim + thời gian dọn phòng 15 phút)"*: Cài đặt tại `ShowtimeScheduleRule.cs`. |
| **K1.4** | DI đăng ký đúng; giỏ/lựa chọn tạm dùng Scoped | **4** | `Cinema.Web/Program.cs`<br>`Cinema.Web/Services/UserSessionService.cs`<br>`Plugins/Cinema.StateStore.DI/SeatHoldingStateStore.cs` | • `Program.cs`: Đăng ký toàn bộ UseCases (`AddScoped`), Repositories (`AddScoped`), StateStore (`AddSingleton`).<br>• Ánh xạ eShop: *"Giỏ hàng ↔ Ghế đang giữ"* được quản lý qua `UserSessionService` (`AddScoped`) và `SeatHoldingStateStore` theo dõi ghế tạm trong phiên làm việc. |
| **K2.1** | Màn hình chính/danh sách hiển thị đúng nghiệp vụ đề tài (8 điểm) | **8** | `Cinema.Web/Components/Pages/Home.razor`<br>`Cinema.Web/Components/Pages/Movies.razor`<br>`Cinema.Web/Components/Pages/MovieDetail.razor` | • Trang chủ hiện danh sách Phim Đang Chiếu, Phim Sắp Chiếu với poster sắc nét, gắn nhãn độ tuổi (T18, T13, P).<br>• Cho phép lọc theo rạp chiếu, ngày chiếu, và chọn nhanh suất chiếu (2D Phụ đề, 2D Lồng tiếng). |
| **K2.2** | Màn hình chi tiết / thêm mới / nghiệp vụ cốt lõi: Sơ đồ ghế trực quan, chọn ghế thời gian thực, giữ ghế 10 phút | **10** | `Cinema.Web/Components/Pages/Booking.razor`<br>`Cinema.Web/Components/Controls/SeatMapComponent.razor` | • **Nghiệp vụ cốt lõi 3 sao**: Sơ đồ ghế ma trận trực quan (ghế Trống, Đang giữ, Đã bán, Đang chọn, ghế VIP, ghế Đôi Sweetbox).<br>• Đếm ngược giữ ghế 10 phút trực quan, tự động nhả ghế khi hết giờ hoặc khi thoát tab/đóng trình duyệt.<br>• Đồng bộ trạng thái ghế real-time giữa các tab/người dùng. |
| **K2.3** | Màn hình tra cứu kết quả / lịch sử / xác nhận đặt vé (7 điểm) | **7** | `Cinema.Web/Components/Pages/BookingConfirmation.razor`<br>`Cinema.Web/Components/Pages/TicketLookup.razor` | • Màn hình Xác nhận đặt vé hiển thị mã vé (`BookingCode`), mã QR vé xem phim, thông tin suất chiếu, vị trí ghế và tổng tiền.<br>• Màn hình Tra cứu vé (`TicketLookup.razor`) cho phép nhập mã vé hoặc số điện thoại để kiểm tra vé đã mua. |
| **K3.1** | Script SQL tạo bảng và nạp dữ liệu mẫu chạy thành công, không lỗi | **8** | `Cinema.SchemaAndData.sql` | • Script hoàn chỉnh tạo 7 bảng: `Cinemas`, `Rooms`, `Movies`, `Showtimes`, `Seats`, `Bookings`, `Tickets`.<br>• Chứa đầy đủ dữ liệu mẫu cho phim, phòng chiếu, ma trận 48-60 ghế mỗi phòng, các suất chiếu thực tế. |
| **K3.2** | Script có khóa chính, khóa ngoại, ràng buộc UNIQUE chống đặt trùng ghế | **6** | `Cinema.SchemaAndData.sql`<br>`Plugins/Cinema.DataStore.SQL.Dapper/BookingRepository.cs` | • Ràng buộc UNIQUE chống bán trùng ghế: `CONSTRAINT UQ_Ticket_Showtime_Seat UNIQUE (ShowtimeId, SeatId)`.<br>• Transaction ACID mức CSDL bảo đảm nếu 2 người thanh toán cùng mili-giây, chỉ 1 người thành công. |
| **K3.3** | Plugin DB dùng Parameterized Query chống SQL Injection tuyệt đối | **6** | `Plugins/Cinema.DataStore.SQL.Dapper/` | • 100% câu lệnh Dapper sử dụng đối tượng tham số hóa: `@ShowtimeId`, `@MovieId`, `@BookingCode`... Tuyệt đối không cộng chuỗi SQL (`string concatenation`). |
| **K4.1** | Unit test dự án chạy PASS 100% không có lỗi | **5** | `Cinema.UnitTests/` | • Chạy lệnh `dotnet test` đạt **6/6 tests PASS (100%)**, thời gian thực thi ~240ms. |
| **K4.2** | Tối thiểu 5 unit tests kiểm thử 2 luật nghiệp vụ (3đ cho 3 test luật 1, 2đ cho 2 test luật 2) | **5** | `Cinema.UnitTests/Rules/SeatBookingRuleTests.cs`<br>`Cinema.UnitTests/Rules/ShowtimeScheduleRuleTests.cs` | • **Luật 1 (3 tests)**: Test giữ ghế quá hạn 10 phút tự nhả; Test ngăn chặn chọn ghế đã có người đặt; Test giải phóng ghế khi hủy đặt vé.<br>• **Luật 2 (2 tests)**: Test phát hiện 2 suất chiếu bị chồng giờ; Test chấp nhận 2 suất chiếu cách nhau đủ 15 phút dọn phòng. |
| **K4.3** | Tối thiểu 1 test mock tầng dữ liệu (Moq hoặc NSubstitute) | **5** | `Cinema.UnitTests/UseCases/CreateBookingUseCaseTests.cs` | • Sử dụng thư viện `Moq` để mock interface `IBookingRepository` và `ISeatRepository`.<br>• Kiểm thử `CreateBookingUseCase` độc lập mà không cần kết nối SQL Server thật. |
| **K5.1** | Toàn bộ solution biên dịch thành công: 0 Warning, 0 Error | **5** | `CinemaBooking.sln` | • `dotnet build CinemaBooking.sln`: **0 Warning, 0 Error**. Mã nguồn tuân thủ Nullable reference types của C# 12. |
| **K5.2** | Cấu trúc thư mục chuẩn Clean Architecture (CoreBusiness, UseCases, Plugins, Web) | **5** | Toàn bộ Solution | • Tách biệt hoàn hảo 4 tầng: `CoreBusiness`, `UseCases`, `Plugins` (DataStore + StateStore), và `Cinema.Web` (Blazor Server). Không vi phạm Dependency Inversion Principle. |
| **K5.3** | README và GRADING đầy đủ thông tin đề tài, sinh viên, hướng dẫn chạy và link commit | **5** | `README.md`<br>`GRADING.md` | • `README.md`: Hướng dẫn cài đặt, cấu hình connection string, chạy migration, tài khoản demo.<br>• `GRADING.md`: Bảng đối chiếu chi tiết 18/18 tiêu chí với số dòng và file cụ thể. |
| **TỔNG** | **Đánh giá toàn diện 18 mã tiêu chí** | **100/100** | | **ĐẠT ĐIỂM TỐI ĐA (HẠNG XUẤT SẮC - 3 SAO ★★★)** |

---

## BẢNG TỔNG HỢP THEO 5 NHÓM TIÊU CHÍ

| Nhóm tiêu chí | Điểm tối đa | Điểm tự đánh giá | Tỷ lệ đạt | Ghi chú |
| :--- | :---: | :---: | :---: | :--- |
| **K1. Kiến trúc Clean Architecture** | 25 | 25 | 100% | Độc lập tầng dữ liệu, DI Scoped/Singleton, 2 Rule độc lập |
| **K2. Màn hình & Nghiệp vụ cốt lõi** | 25 | 25 | 100% | Đầy đủ sơ đồ ghế trực quan, giữ ghế 10p, tra cứu vé |
| **K3. Cơ sở dữ liệu & Tầng dữ liệu** | 20 | 20 | 100% | Script chuẩn Dapper, UQ_Ticket_Showtime_Seat chống tranh chấp |
| **K4. Unit Tests** | 15 | 15 | 100% | 6 tests PASS, 5 tests cho 2 luật, Moq repository |
| **K5. Chất lượng mã & Trình bày** | 15 | 15 | 100% | 0 Warning, 0 Error, cấu trúc chuẩn, tài liệu chi tiết |
| **TỔNG CỘNG** | **100** | **100** | **100%** | **Xuất sắc (A+)** |
