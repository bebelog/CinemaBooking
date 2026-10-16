# BẢNG ĐỐI CHIẾU TIÊU CHÍ ĐÁNH GIÁ ĐỒ ÁN CAPSTONE — ECO2415

* **Đề tài**: **14 · Rạp phim: Chọn ghế & Đặt vé — Cinema Seat Booking**
* **Độ khó**: **★★★ (Nâng cao)**
* **Sinh viên thực hiện**: **Nguyễn Viết Mẫn** (MSSV: **23K4080026** - HUE)
* **GitHub Repository**: [https://github.com/nguyenman-hue/CinemaBooking.git](https://github.com/nguyenman-hue/CinemaBooking.git)
* **Tổng điểm tự đánh giá**: **100/100 Điểm (Đạt Hạng Xuất Sắc - 18/18 Tiêu chí Đạt Đủ)**

---

## BẢNG TỔNG HỢP THEO 5 NHÓM TIÊU CHÍ (RUBRIC MỤC 11)

| Nhóm tiêu chí | Điểm tối đa | Điểm tự đánh giá | Tỷ lệ | Đánh giá tổng quan |
| :--- | :---: | :---: | :---: | :--- |
| **K1. Kiến trúc Clean Architecture & Code Quality** | **25** | **25** | 100% | 4 project chuẩn mực, Dependency Rule, 2 Rule độc lập, DI Scoped |
| **K2. Cơ sở Dữ liệu & Tính toàn vẹn Dữ liệu** | **25** | **25** | 100% | 7 bảng 3NF, Transaction Header-Line, Dapper parameterized 100% |
| **K3. Giao diện & Trải nghiệm Người dùng** | **20** | **20** | 100% | 3 component tái sử dụng, EditForm + DataAnnotations, mobile 390px |
| **K4. Bảo mật, Phân quyền & Tính năng Nâng cao / AI** | **15** | **15** | 100% | 2 vai trò (Customer & Admin), chặn truy cập trái phép, Gemini 2.5 Flash |
| **K5. Hồ sơ Thiết kế (UML) & Thuyết trình Vấn đáp** | **15** | **15** | 100% | 3 sơ đồ PlantUML chuẩn, commit rải trên 3 tuần, tài liệu đầy đủ |
| **TỔNG CỘNG** | **100** | **100** | **100%** | **Hạng Xuất Sắc (A+)** |

---

## BẢNG BẰNG CHỨNG CHI TIẾT 18 MÃ KIỂM TRA (MỤC 12 - TRANG 13-14)

### NHÓM K1: KIẾN TRÚC CLEAN ARCHITECTURE & CODE QUALITY (25 ĐIỂM)

| Mã | Tiêu chí đánh giá | Điểm | File & Dòng Code làm bằng chứng | Mô tả chi tiết minh chứng khi vấn đáp |
| :---: | :--- | :---: | :--- | :--- |
| **K1.1** | Đủ 4 project; CoreBusiness không tham chiếu project nào; UseCases chỉ tham chiếu CoreBusiness | **8/8** | `Cinema.CoreBusiness/Cinema.CoreBusiness.csproj`<br>`Cinema.UseCases/Cinema.UseCases.csproj`<br>`Cinema.Web/Cinema.Web.csproj` | • `Cinema.CoreBusiness.csproj` không có bất kỳ thẻ `<ProjectReference>` nào.<br>• `Cinema.UseCases.csproj` chỉ tham chiếu duy nhất `Cinema.CoreBusiness.csproj`.<br>• `Cinema.Web.csproj` đóng vai trò Composition Root, liên kết Plugins tại `Program.cs`. |
| **K1.2** | Interface repository nằm ở UseCases, Plugin cài đặt; file .razor inject interface, không inject lớp cụ thể | **7/7** | `Cinema.UseCases/PluginInterfaces/DataStore/`<br>`Plugins/Cinema.DataStore.SQL.Dapper/Repositories/`<br>`Cinema.Web/Components/Pages/BookingPage.razor` (Dòng 2-5) | • 5 Interface: `IMovieRepository`, `IShowtimeRepository`, `ISeatRepository`, `IBookingRepository`, `IUserRepository` nằm trọn trong tầng `Cinema.UseCases`.<br>• Tầng Web Razor Components chỉ `@inject` UseCase Interface (VD: `ICreateBookingUseCase`, `IViewShowtimesUseCase`), tuyệt đối không inject lớp cụ thể hay Repository. |
| **K1.3** | 2 luật nghiệp vụ của đề cài trong CoreBusiness / UseCases, không nằm trong .razor (3 điểm mỗi luật) | **6/6** | **Luật 1**: `Cinema.CoreBusiness/Rules/SeatBookingRule.cs` (Dòng 12-140)<br>**Luật 2**: `Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs` (Dòng 10-65)<br>`Cinema.UseCases/Bookings/CreateBookingUseCase.cs` (Dòng 35-65) | • **Luật 1 (Tranh chấp ghế & Giữ ghế 10p)**: Chặn bán trùng ghế, chống tranh chấp ghế đang giữ, giới hạn 1-8 ghế và chống để lại ghế trống đơn lẻ (Single Orphan Seat).<br>• **Luật 2 (Không chồng giờ suất chiếu)**: Các suất cùng phòng cách nhau tối thiểu thời lượng phim + 15 phút dọn phòng, chặn xếp lịch trong quá khứ và ngoài giờ mở cửa (08:00 - 23:30). |
| **K1.4** | DI đăng ký đủ; giỏ/lựa chọn tạm dùng Scoped | **4/4** | `Cinema.Web/Program.cs` (Dòng 35-58)<br>`Cinema.Web/Services/UserSessionService.cs`<br>`Plugins/Cinema.StateStore.DI/SeatHoldingStateStore.cs` | • Ánh xạ eShop: *"Giỏ hàng ↔ Ghế đang giữ"* được quản lý bằng `UserSessionService` đăng ký `AddScoped` (kết hợp Cookie session) và `SeatHoldingStateStore` đăng ký `AddSingleton` (quản lý tranh chấp đồng thời giữa các người dùng). |

---

### NHÓM K2: CƠ SỞ DỮ LIỆU & TÍNH TOÀN VẸN DỮ LIỆU (25 ĐIỂM)

| Mã | Tiêu chí đánh giá | Điểm | File & Dòng Code làm bằng chứng | Mô tả chi tiết minh chứng khi vấn đáp |
| :---: | :--- | :---: | :--- | :--- |
| **K2.1** | Từ 5 bảng, chuẩn 3NF, có khóa ngoại; script tạo bảng + dữ liệu mẫu trong repo chạy được | **8/8** | `Cinema.SchemaAndData.sql` (Toàn bộ file) | • Script hoàn chỉnh tạo 7 bảng chuẩn 3NF: `[User]`, `Movie`, `Auditorium`, `Seat`, `Showtime`, `Booking`, `Ticket`.<br>• Chạy mượt mà trên SQL Server trống, tạo sẵn khóa ngoại và dữ liệu mẫu phong phú. |
| **K2.2** | Cặp header–line ghi trong 1 transaction | **6/6** | `Plugins/Cinema.DataStore.SQL.Dapper/Repositories/BookingRepository.cs` (Dòng 28-62) | • Cặp `Booking` (Header) và `Ticket` (Lines) được thực thi trong một `SqlTransaction` ACID duy nhất.<br>• Ràng buộc `UQ_Ticket_Showtime_Seat` đảm bảo nếu 2 khách hàng thanh toán cùng 1 ghế ở cùng mili-giây, giao dịch thứ hai bị Rollback ngay lập tức. |
| **K2.3** | Mọi truy vấn tham số hóa, không nối chuỗi SQL | **5/5** | `Plugins/Cinema.DataStore.SQL.Dapper/Repositories/` | • 100% câu lệnh Dapper sử dụng tham số hóa (`@ShowtimeId`, `@BookingReference`, `@UserId`...). Tuyệt đối không có cộng chuỗi SQL (`string concatenation`), triệt tiêu hoàn toàn lỗ hổng SQL Injection. |
| **K2.4** | Luồng giao dịch chính chạy end-to-end và dữ liệu nằm trong SQL Server | **6/6** | Demo giao diện trực tiếp trên trình duyệt & xem bảng trong SQL Server | • Khách chọn suất $\rightarrow$ Chọn ghế $\rightarrow$ Điền thông tin $\rightarrow$ Đặt vé $\rightarrow$ Dữ liệu ghi thành công vào bảng `Booking` và `Ticket` trong CSDL `CinemaDB`. |

---

### NHÓM K3: GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (20 ĐIỂM)

| Mã | Tiêu chí đánh giá | Điểm | File & Dòng Code làm bằng chứng | Mô tả chi tiết minh chứng khi vấn đáp |
| :---: | :--- | :---: | :--- | :--- |
| **K3.1** | Ít nhất 3 component tái sử dụng có [Parameter] / EventCallback | **6/6** | 1. `Cinema.Web/Components/Controls/SeatMapComponent.razor`<br>2. `Cinema.Web/Components/Controls/MovieCardComponent.razor`<br>3. `Cinema.Web/Components/Controls/ConfirmModalComponent.razor` | • **Component 1 (SeatMap)**: `[Parameter] ShowtimeId`, `[Parameter] BasePrice`, `EventCallback<List<SeatDto>> OnSelectionChanged`.<br>• **Component 2 (MovieCard)**: `[Parameter] Movie`, `EventCallback<Movie> OnSelectMovie`.<br>• **Component 3 (ConfirmModal)**: `[Parameter] Title`, `[Parameter] Message`, `EventCallback<bool> OnResult`. |
| **K3.2** | EditForm + DataAnnotations; lỗi nhập liệu hiển thị cạnh ô nhập | **5/5** | `Cinema.Web/Components/Pages/BookingPage.razor` (Dòng 145-185)<br>`Cinema.Web/Components/Pages/RegisterPage.razor` (Dòng 40-105) | • Dùng `<EditForm Model="..." OnValidSubmit="...">`, `<DataAnnotationsValidator />`, `<InputText>` và `<ValidationMessage For="..." />`. Khi để trống hoặc gõ sai định dạng email/sđt, thông báo lỗi đỏ hiện ngay dưới ô nhập liệu. |
| **K3.3** | Ca vi phạm 2 luật nghiệp vụ hiện thông báo thân thiện, không văng lỗi | **4/4** | `Cinema.Web/Components/Pages/BookingPage.razor`<br>`Cinema.Web/Components/Pages/Admin/ManageShowtimesPage.razor` | • **Vi phạm Luật 1**: Thông báo *"Việc chọn ghế của bạn không được để trống 1 ghế ở bên trái, giữa hoặc bên phải trên cùng một hàng ghế mà bạn vừa chọn (Để lại ghế trống đơn lẻ {Tên_Ghế})!"*<br>• **Vi phạm Luật 2**: Thông báo lỗi chồng giờ hoặc phòng chưa dọn xong thân thiện trong Modal thêm suất chiếu. |
| **K3.4** | Dùng được trên màn hình điện thoại (DevTools 390px) | **5/5** | `Cinema.Web/wwwroot/app.css` & Bootstrap 5 Responsive Grid | • Kiểm thử trên DevTools iPhone 12/13/14 Pro (390px): Sơ đồ ghế cuộn ngang mượt mà, thanh tóm tắt đặt vé và menu điều hướng co giãn hoàn hảo. |

---

### NHÓM K4: BẢO MẬT, PHÂN QUYỀN & TÍNH NĂNG NÂNG CAO / AI (15 ĐIỂM)

| Mã | Tiêu chí đánh giá | Điểm | File & Dòng Code làm bằng chứng | Mô tả chi tiết minh chứng khi vấn đáp |
| :---: | :--- | :---: | :--- | :--- |
| **K4.1** | Đăng nhập, đăng xuất; ít nhất 2 vai trò | **4/4** | `Cinema.Web/Components/Pages/LoginPage.razor`<br>`Cinema.Web/Components/Pages/RegisterPage.razor`<br>`Cinema.Web/Services/CustomAuthStateProvider.cs` | • Có 2 vai trò rõ ràng: **Admin** (`admin` / `admin123`) và **Customer** (`khachhang` / `khach123` hoặc tài khoản vừa đăng ký).<br>• Hỗ trợ đăng ký tài khoản khách hàng mới, đăng nhập, đăng xuất và xem "Vé Của Tôi" (`/my-bookings`). |
| **K4.2** | Tài khoản thường gõ thẳng URL trang quản trị bị chặn | **5/5** | `Cinema.Web/Components/Routes.razor` (Dòng 6-22)<br>`Cinema.Web/Components/Pages/Admin/*.razor` | • Toàn bộ trang Admin được bảo vệ bởi `@attribute [Authorize(Roles = "Admin")]`.<br>• Khi đăng nhập vai trò `Customer`, nếu gõ thẳng URL `/admin/movies` hay `/admin/showtimes`, `AuthorizeRouteView` lập tức chặn và hiển thị thông báo: *"🔒 Yêu Cầu Quyền Quản Trị"*. |
| **K4.3** | Tính năng nâng cao: AI đạt đủ mục 10, hoặc xuất báo cáo (Excel/PDF) từ dữ liệu thật | **6/6** | `Cinema.UseCases/AI/SemanticMovieSearchUseCase.cs`<br>`Cinema.Web/Components/Controls/AiMovieAssistant.razor`<br>`Cinema.Web/Components/Pages/Admin/OccupancyReportPage.razor` | • Tích hợp **Google Gemini 2.5 Flash API** thực hiện Trợ lý AI Semantic Search: tìm phim theo ngữ nghĩa tự nhiên (*"phim hoạt hình vui nhộn cho gia đình"*).<br>• Tuân thủ nghiêm ngặt Mục 10: API Key đặt trong User Secrets / Local, đặt trong UseCase riêng, có xử lý lỗi khi mất mạng.<br>• Hỗ trợ Báo cáo tỷ lệ lấp đầy phòng chiếu từ dữ liệu SQL thật. |

---

### NHÓM K5: HỒ SƠ THIẾT KẾ (UML) & THUYẾT TRÌNH VẤN ĐÁP (15 ĐIỂM)

| Mã | Tiêu chí đánh giá | Điểm | File & Dòng Code làm bằng chứng | Mô tả chi tiết minh chứng khi vấn đáp |
| :---: | :--- | :---: | :--- | :--- |
| **K5.1** | Use Case, Class, Sequence (PlantUML) khớp tên lớp và luồng trong code | **6/6** | `docs/uml/usecase_diagram.puml`<br>`docs/uml/class_diagram.puml`<br>`docs/uml/sequence_diagram.puml` | • Đầy đủ 3 sơ đồ PlantUML chuẩn: Use Case (2 Actor: Customer & Admin), Class Diagram (khớp 100% tên lớp 4 tầng Clean Architecture), Sequence Diagram (chi tiết luồng giao dịch Header-Line với Transaction và giữ ghế). |
| **K5.2** | Vấn đáp: mỗi thành viên chỉ đúng dòng cài luật và giải thích Dependency Rule | **6/6** | Xem K1.3 và phần hướng dẫn vấn đáp bên dưới | • Nắm vững vị trí code: Luật 1 tại `SeatBookingRule.cs`, Luật 2 tại `ShowtimeScheduleRule.cs`, giải thích rõ Dependency Rule: Tầng trong không phụ thuộc tầng ngoài. |
| **K5.3** | Mọi thành viên có commit, rải trên ít nhất 3 tuần | **3/3** | `git log --date=short`<br>`git shortlog -sn` | • Lịch sử commit rải đều qua 3 tuần: Tuần 05 (Sprint 1), Tuần 06 (Sprint 2), Tuần 07 (Bảo vệ), với hơn 25 commit có ý nghĩa rõ ràng. |

---

## BỘ KIỂM THỬ TỰ ĐỘNG (UNIT TESTS): 29/29 TESTS PASS 100%

Lệnh kiểm tra: `dotnet test Cinema.UnitTests/Cinema.UnitTests.csproj`

* **Rule1_SeatConflictTests.cs (10 tests)**: Kiểm thử toàn diện Luật 1 (chặn bán trùng ghế, tranh chấp ghế đang giữ, giới hạn 1-8 vé, chống để lại ghế trống đơn lẻ).
* **Rule2_ShowtimeOverlapTests.cs (9 tests)**: Kiểm thử toàn diện Luật 2 (chồng giờ chiếu, 15 phút dọn phòng, chặn quá khứ, giờ mở cửa rạp).
* **AiSemanticSearchTests.cs (3 tests)**: Kiểm thử trợ lý AI và xử lý sự cố mạng.
* **BookingEmailNotificationTests.cs (3 tests)**: Kiểm thử gửi email vé điện tử tự động và bảo đảm tính chịu lỗi của giao dịch.
* **UserRegistrationTests.cs (4 tests)**: Kiểm thử đăng ký thành viên, validation tên đăng nhập, email trùng lặp và độ dài mật khẩu.
