# BẢNG ĐỐI CHIẾU MINH CHỨNG MÃ KIỂM TRA (K1.1 – K5.3)

* **Đề tài**: **14 · Rạp phim: Chọn ghế & Đặt vé (Cinema Seat Booking)**
* **Sinh viên thực hiện**: **Nguyễn Viết Mẫn** (MSSV: **23K4080026** - Lớp: K46/21.02)
* **GitHub Repository**: [https://github.com/nguyenman-hue/CinemaBooking](https://github.com/nguyenman-hue/CinemaBooking)

---

## BẢNG MINH CHỨNG CHI TIẾT THEO MỤC 12

### NHÓM K1: KIẾN TRÚC CLEAN ARCHITECTURE & CHẤT LƯỢNG MÃ NGUỒN

| Mã kiểm tra | Nội dung tiêu chí | Đường dẫn file làm bằng chứng | Số dòng làm bằng chứng | Mô tả chi tiết minh chứng |
| :---: | :--- | :--- | :---: | :--- |
| **K1.1** | Đủ 4 project; CoreBusiness không tham chiếu project nào; UseCases chỉ tham chiếu CoreBusiness | `Cinema.CoreBusiness/Cinema.CoreBusiness.csproj`<br>`Cinema.UseCases/Cinema.UseCases.csproj`<br>`Cinema.Web/Cinema.Web.csproj`<br>`CinemaBooking.sln` | Toàn bộ file | • `Cinema.CoreBusiness.csproj` độc lập tuyệt đối, không có thẻ `<ProjectReference>` nào.<br>• `Cinema.UseCases.csproj` chỉ tham chiếu duy nhất `Cinema.CoreBusiness`.<br>• `Cinema.Web.csproj` đóng vai trò Composition Root, liên kết các Plugin tại `Program.cs`. |
| **K1.2** | Interface repository nằm ở UseCases, Plugin cài đặt; file .razor inject interface, không inject lớp cụ thể | `Cinema.UseCases/PluginInterfaces/DataStore/`<br>`Plugins/Cinema.DataStore.SQL.Dapper/Repositories/`<br>`Cinema.Web/Components/Pages/BookingPage.razor` | Dòng 2–10 | • Tầng UseCases định nghĩa các Interface: `IMovieRepository`, `IShowtimeRepository`, `ISeatRepository`, `IBookingRepository`, `IUserRepository`.<br>• Tầng Plugins hiện thực các Interface bằng Dapper.<br>• Các trang `.razor` chỉ `@inject` UseCase Interface, không bao giờ inject lớp cụ thể hay Repository. |
| **K1.3** | 2 luật nghiệp vụ của đề cài trong CoreBusiness / UseCases, không nằm trong .razor | `Cinema.CoreBusiness/Rules/SeatBookingRule.cs`<br>`Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs`<br>`Cinema.UseCases/Bookings/CreateBookingUseCase.cs` | `SeatBookingRule.cs`: Dòng 14–210<br>`ShowtimeScheduleRule.cs`: Dòng 10–65<br>`CreateBookingUseCase.cs`: Dòng 35–65 | • **Luật 1 (Tranh chấp ghế & Giữ ghế 10 phút & Chống ghế đơn lẻ)**: Cài đặt hoàn chỉnh trong `SeatBookingRule.cs`, chặn bán trùng ghế, kiểm tra ghế đang bị giữ tạm thời và chống bỏ lại ghế trống đơn lẻ.<br>• **Luật 2 (Không chồng giờ chiếu)**: Cài đặt trong `ShowtimeScheduleRule.cs`, đảm bảo khoảng cách giữa các suất cùng phòng $\ge$ thời lượng phim + 15 phút dọn phòng. |
| **K1.4** | Dependency Injection đăng ký đủ; giỏ/lựa chọn tạm dùng Scoped | `Cinema.Web/Program.cs`<br>`Cinema.Web/Services/UserSessionService.cs`<br>`Plugins/Cinema.StateStore.DI/SeatHoldingStateStore.cs` | `Program.cs`: Dòng 32–45, 87–90 | • Giỏ hàng/lựa chọn ghế tạm thời dùng `UserSessionService` đăng ký `AddScoped` (kết hợp Cookie Session).<br>• Bộ lưu trữ giữ chỗ thời gian thực `SeatHoldingStateStore` đăng ký `AddSingleton` để quản lý tranh chấp đồng thời giữa các người dùng. |

---

### NHÓM K2: CƠ SỞ DỮ LIỆU & TÍNH TOÀN VẸN DỮ LIỆU

| Mã kiểm tra | Nội dung tiêu chí | Đường dẫn file làm bằng chứng | Số dòng làm bằng chứng | Mô tả chi tiết minh chứng |
| :---: | :--- | :--- | :---: | :--- |
| **K2.1** | Từ 5 bảng trở lên, chuẩn 3NF, có khóa ngoại; script tạo bảng và dữ liệu mẫu nằm trong repo chạy được | `Cinema.SchemaAndData.sql` | Dòng 1–350 | • CSDL quan hệ gồm 7 bảng chuẩn 3NF: `[User]`, `Movie`, `Auditorium`, `Seat`, `Showtime`, `Booking`, `Ticket`.<br>• Đầy đủ các ràng buộc khóa chính, khóa ngoại `FOREIGN KEY` và dữ liệu mẫu chạy thành công trên SQL Server trống. |
| **K2.2** | Cặp header–line ghi trong 1 transaction | `Plugins/Cinema.DataStore.SQL.Dapper/Repositories/BookingRepository.cs`<br>`Cinema.SchemaAndData.sql` | `BookingRepository.cs`: Dòng 25–65<br>`Cinema.SchemaAndData.sql`: Dòng 82 | • Cặp `Booking` (Header) và `Ticket` (Line Items) được thực thi trong cùng một `SqlTransaction` ACID duy nhất.<br>• Ràng buộc Unique `UQ_Ticket_Showtime_Seat` đảm bảo nếu 2 giao dịch tranh chấp cùng 1 ghế ở cùng mili-giây thì giao dịch thứ hai bị Rollback ngay lập tức. |
| **K2.3** | Mọi truy vấn tham số hóa, không nối chuỗi SQL | `Plugins/Cinema.DataStore.SQL.Dapper/Repositories/` | Toàn bộ các file Repository | • 100% câu truy vấn Dapper sử dụng Parameterized Query (ví dụ: `@ShowtimeId`, `@BookingReference`, `@UserId`). Tuyệt đối không nối chuỗi SQL trực tiếp, ngăn chặn hoàn toàn SQL Injection. |
| **K2.4** | Luồng giao dịch chính chạy end-to-end và dữ liệu nằm trong SQL Server | Giao diện đặt vé Web & Bảng `Booking`, `Ticket` trong CSDL `CinemaDB` | Toàn luồng | • Người dùng chọn phim $\rightarrow$ chọn suất chiếu $\rightarrow$ chọn ghế $\rightarrow$ điền thông tin $\rightarrow$ thanh toán $\rightarrow$ dữ liệu được lưu thành công vào các bảng trong SQL Server. |

---

### NHÓM K3: GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG

| Mã kiểm tra | Nội dung tiêu chí | Đường dẫn file làm bằng chứng | Số dòng làm bằng chứng | Mô tả chi tiết minh chứng |
| :---: | :--- | :--- | :---: | :--- |
| **K3.1** | Ít nhất 3 component tái sử dụng có `[Parameter]` / `EventCallback` | 1. `Cinema.Web/Components/Controls/SeatMapComponent.razor`<br>2. `Cinema.Web/Components/Controls/MovieCardComponent.razor`<br>3. `Cinema.Web/Components/Controls/ConfirmModalComponent.razor` | `SeatMapComponent.razor`: Dòng 80–110<br>`MovieCardComponent.razor`: Dòng 20–35<br>`ConfirmModalComponent.razor`: Dòng 15–30 | • `SeatMapComponent`: `[Parameter] ShowtimeId`, `EventCallback<List<SeatDto>> OnSelectionChanged`.<br>• `MovieCardComponent`: `[Parameter] Movie`, `EventCallback<Movie> OnSelectMovie`.<br>• `ConfirmModalComponent`: `[Parameter] Title`, `EventCallback<bool> OnResult`. |
| **K3.2** | EditForm + DataAnnotations; lỗi nhập liệu hiển thị cạnh ô nhập | `Cinema.Web/Components/Pages/BookingPage.razor`<br>`Cinema.Web/Components/Pages/RegisterPage.razor` | `BookingPage.razor`: Dòng 140–185<br>`RegisterPage.razor`: Dòng 40–105 | • Sử dụng `<EditForm Model="..." OnValidSubmit="...">`, `<DataAnnotationsValidator />`, `<InputText>` và `<ValidationMessage For="..." />`. Lỗi nhập liệu hiển thị trực tiếp ngay dưới trường tương ứng. |
| **K3.3** | Ca vi phạm 2 luật nghiệp vụ hiện thông báo thân thiện, không văng lỗi | `Cinema.Web/Components/Controls/SeatMapComponent.razor`<br>`Cinema.Web/Components/Pages/BookingPage.razor`<br>`Cinema.Web/Components/Pages/Admin/ManageShowtimesPage.razor` | `SeatMapComponent.razor`: Dòng 25–40, 160–185<br>`ManageShowtimesPage.razor`: Dòng 90–120 | • Vi phạm chọn ghế để lại ghế đơn lẻ hoặc tranh chấp ghế đang giữ: hiển thị banner cảnh báo thân thiện và tự động tắt sau 4 giây.<br>• Vi phạm trùng lịch chiếu: hiển thị thông báo chi tiết trên Modal quản lý, không để lộ crash hay ngoại lệ hệ thống ra màn hình. |
| **K3.4** | Dùng được trên màn hình điện thoại (DevTools 390px) | `Cinema.Web/wwwroot/app.css`<br>`Cinema.Web/Components/Controls/SeatMapComponent.razor` | Toàn bộ CSS bố cục responsive | • Sơ đồ ghế hỗ trợ cuộn ngang linh hoạt (`overflow-x: auto`), thanh tóm tắt đơn vé và menu điều hướng co giãn hoàn chỉnh trên màn hình di động độ rộng 390px. |

---

### NHÓM K4: BẢO MẬT, PHÂN QUYỀN & TÍNH NĂNG NÂNG CAO

| Mã kiểm tra | Nội dung tiêu chí | Đường dẫn file làm bằng chứng | Số dòng làm bằng chứng | Mô tả chi tiết minh chứng |
| :---: | :--- | :--- | :---: | :--- |
| **K4.1** | Đăng nhập, đăng xuất; ít nhất 2 vai trò | `Cinema.Web/Components/Pages/LoginPage.razor`<br>`Cinema.Web/Components/Pages/RegisterPage.razor`<br>`Cinema.Web/Services/CustomAuthStateProvider.cs` | `LoginPage.razor`: Dòng 20–85<br>`CustomAuthStateProvider.cs`: Dòng 15–70 | • Cung cấp 2 vai trò phân quyền: **Admin** (`admin` / `admin123`) và **Customer** (`khachhang` / `khach123`).<br>• Hỗ trợ đăng ký người dùng mới, đăng nhập, đăng xuất và quản lý vé cá nhân tại `/my-bookings`. |
| **K4.2** | Tài khoản thường gõ thẳng URL trang quản trị bị chặn | `Cinema.Web/Components/Routes.razor`<br>`Cinema.Web/Components/Pages/Admin/*.razor` | `Routes.razor`: Dòng 6–25<br>Tất cả file Admin: Dòng 1–3 | • Các trang Admin được bảo vệ bởi `@attribute [Authorize(Roles = "Admin")]`.<br>• Khi người dùng không có quyền quản trị gõ thẳng URL (như `/admin/movies`, `/admin/showtimes`), `AuthorizeRouteView` chặn và chuyển vào nhánh `<NotAuthorized>` hiển thị màn hình từ chối truy cập. |
| **K4.3** | Tính năng nâng cao: AI hoặc Báo cáo từ dữ liệu thật | `Cinema.UseCases/AI/SemanticMovieSearchUseCase.cs`<br>`Cinema.Web/Components/Controls/AiMovieAssistant.razor`<br>`Cinema.Web/Components/Pages/Admin/OccupancyReportPage.razor` | `SemanticMovieSearchUseCase.cs`: Dòng 15–120<br>`OccupancyReportPage.razor`: Dòng 20–95 | • Tích hợp Trợ lý AI tìm kiếm phim theo ngữ nghĩa tự nhiên sử dụng **Google Gemini 2.5 Flash API**.<br>• Chức năng Báo cáo thống kê tỷ lệ lấp đầy phòng chiếu tính toán từ dữ liệu thực trong CSDL SQL Server. |

---

### NHÓM K5: HỒ SƠ THIẾT KẾ (UML) & MINH CHỨNG LÀM VIỆC

| Mã kiểm tra | Nội dung tiêu chí | Đường dẫn file làm bằng chứng | Số dòng làm bằng chứng | Mô tả chi tiết minh chứng |
| :---: | :--- | :--- | :---: | :--- |
| **K5.1** | Use Case, Class, Sequence (PlantUML) khớp tên lớp và luồng trong code | `docs/uml/usecase_diagram.puml`<br>`docs/uml/class_diagram.puml`<br>`docs/uml/sequence_diagram.puml` | Toàn bộ các file trong `docs/uml/` | • 3 sơ đồ PlantUML chuẩn xác: Use Case Diagram (Customer & Admin), Class Diagram (khớp 100% tên lớp 4 tầng Clean Architecture), Sequence Diagram (mô tả luồng giao dịch đặt vé kèm Transaction và giữ chỗ). |
| **K5.2** | Vấn đáp: chỉ đúng dòng cài luật và giải thích Dependency Rule | `Cinema.CoreBusiness/Rules/SeatBookingRule.cs`<br>`Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs` | `SeatBookingRule.cs`: Dòng 14–210<br>`ShowtimeScheduleRule.cs`: Dòng 10–65 | • Sẵn sàng chỉ chính xác vị trí code cài đặt 2 luật nghiệp vụ và giải thích nguyên tắc đảo ngược phụ thuộc (Dependency Inversion / Clean Architecture). |
| **K5.3** | Lịch sử commit theo tuần của các thành viên | `git log --date=short` | Git commit history | • Lịch sử commit minh bạch rải đều qua các tuần thực hiện đồ án với thông điệp rõ ràng theo chuẩn Conventional Commits. |
