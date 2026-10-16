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
| **K1.1** | Đủ 4 project; CoreBusiness không tham chiếu project nào; UseCases chỉ tham chiếu CoreBusiness | **8/8** | `Cinema.CoreBusiness/Cinema.CoreBusiness.csproj`<br>`Cinema.UseCases/Cinema.UseCases.csproj`<br>`Cinema.Web/Cinema.Web.csproj` | • Cấu trúc chuẩn mực 4 project theo mẫu eShop: `CoreBusiness`, `UseCases`, `Web`, `Plugins`.<br>• `Cinema.CoreBusiness.csproj` không có bất kỳ thẻ `<ProjectReference>` nào.<br>• `Cinema.UseCases.csproj` chỉ tham chiếu duy nhất `Cinema.CoreBusiness.csproj`.<br>• `Cinema.Web.csproj` đóng vai trò Composition Root, liên kết Plugins tại `Program.cs`. |
| **K1.2** | Interface repository nằm ở UseCases, Plugin cài đặt; file .razor inject interface, không inject lớp cụ thể | **7/7** | `Cinema.UseCases/PluginInterfaces/DataStore/`<br>`Plugins/Cinema.DataStore.SQL.Dapper/Repositories/`<br>`Cinema.Web/Components/Pages/BookingPage.razor` (Dòng 2-5) | • 5 Interface: `IMovieRepository`, `IShowtimeRepository`, `ISeatRepository`, `IBookingRepository`, `IUserRepository` nằm trọn trong tầng `Cinema.UseCases`.<br>• Tầng Web Razor Components chỉ `@inject` UseCase Interface (VD: `ICreateBookingUseCase`, `IViewShowtimesUseCase`), tuyệt đối không inject lớp cụ thể hay Repository. |
| **K1.3** | 2 luật nghiệp vụ của đề cài trong CoreBusiness / UseCases, không nằm trong .razor (3 điểm mỗi luật) | **6/6** | **Luật 1**: `Cinema.CoreBusiness/Rules/SeatBookingRule.cs`<br>**Luật 2**: `Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs`<br>`Cinema.UseCases/Bookings/CreateBookingUseCase.cs` | • **Luật 1 (Tranh chấp ghế & Giữ ghế 10p)**: Chặn bán trùng ghế, chống tranh chấp ghế đang giữ, giới hạn 1-8 ghế, kiểm tra ghế đơn lẻ bị cô lập (Single Orphan Seat) và thuật toán tương tác thời gian thực kiểu Cinestar.<br>• **Luật 2 (Không chồng giờ suất chiếu)**: Các suất cùng phòng cách nhau tối thiểu thời lượng phim + 15 phút dọn phòng, chặn xếp lịch trong quá khứ và ngoài giờ mở cửa (08:00 - 23:30). |
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
| **K3.2** | EditForm + DataAnnotations; lỗi nhập liệu hiển thị cạnh ô nhập | **5/5** | `Cinema.Web/Components/Pages/BookingPage.razor`<br>`Cinema.Web/Components/Pages/RegisterPage.razor` | • Dùng `<EditForm Model="..." OnValidSubmit="...">`, `<DataAnnotationsValidator />`, `<InputText>` và `<ValidationMessage For="..." />`. Khi để trống hoặc gõ sai định dạng email/sđt, thông báo lỗi đỏ hiện ngay dưới ô nhập liệu. |
| **K3.3** | Ca vi phạm 2 luật nghiệp vụ hiện thông báo thân thiện, không văng lỗi | **4/4** | `Cinema.Web/Components/Controls/SeatMapComponent.razor`<br>`Cinema.Web/Components/Pages/BookingPage.razor`<br>`Cinema.Web/Components/Pages/Admin/ManageShowtimesPage.razor` | • **Vi phạm Luật 1**: Thông báo tự tắt sau 4s khi vi phạm chọn ghế bỏ trống đơn lẻ hoặc tranh chấp ghế đang giữ.<br>• **Vi phạm Luật 2**: Thông báo lỗi trùng giờ chiếu / phòng chưa dọn xong hiển thị rõ ràng trên modal Admin. Không bao giờ văng Exception ra màn hình trắng. |
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

## HƯỚNG DẪN TRẢ LỜI VẤN ĐÁP 100% ĂN ĐIỂM (CHEATSHEET KHI GIẢNG VIÊN HỎI)

### 1. Câu hỏi về Kiến trúc Clean Architecture & Dependency Rule
* **Giáo viên hỏi**: *"Tại sao tầng CoreBusiness không được tham chiếu bất kỳ thư viện hay tầng nào khác?"*
* **Trả lời**: *"Thưa thầy/cô, theo nguyên lý Dependency Rule của Clean Architecture, Domain/CoreBusiness là hạt nhân chứa các Entity và quy tắc nghiệp vụ cốt lõi nhất của hệ thống. Nó phải độc lập hoàn toàn với công nghệ giao diện (Blazor), cơ sở dữ liệu (SQL Server/Dapper) hay thư viện bên ngoài. Nhờ đó, nếu sau này thay đổi CSDL sang MongoDB hay đổi UI sang Mobile App MAUI thì toàn bộ logic đặt vé và kiểm tra ghế trong CoreBusiness vẫn giữ nguyên 100% mà không phải sửa một dòng code nào."*

### 2. Câu hỏi về Cài đặt 2 Luật Nghiệp vụ
* **Giáo viên hỏi**: *"2 luật nghiệp vụ của đề tài được cài đặt ở đâu? Hãy mở file code và chỉ ra?"*
* **Trả lời**:
  * **Luật 1 (Tranh chấp ghế & Giữ ghế 10 phút & Chống ghế đơn lẻ)**: Cài trong `Cinema.CoreBusiness/Rules/SeatBookingRule.cs`.
    - Phương thức `ValidateSeatsAvailability(...)`: Kiểm tra ghế đã bán trong CSDL và ghế đang bị người khác giữ tạm thời trong vòng 10 phút (`SeatHoldingStateStore`).
    - Phương thức `ValidateSingleOrphanSeats(...)` và `ValidateInteractiveSeatSelection(...)`: Thuật toán duyệt qua từng hàng ghế theo thứ tự số ghế, kiểm tra không cho phép để lại 1 ghế trống đơn lẻ bị kẹp giữa 2 ghế đã chọn hoặc mép tường.
  * **Luật 2 (Không chồng giờ chiếu & 15 phút dọn phòng)**: Cài trong `Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs`.
    - Kiểm tra `existing.StartTime < newEnd && newStart < existingEnd`: Đảm bảo 2 khoảng thời gian chiếu không giao nhau.
    - Thời lượng dọn dẹp phòng: `newShowtime.Movie.DurationMinutes + 15` phút.

### 3. Câu hỏi về Toàn vẹn Dữ liệu & Transaction Header-Line
* **Giáo viên hỏi**: *"Cặp Header-Line (Booking - Ticket) được lưu như thế nào để đảm bảo 2 khách hàng không bao giờ mua trùng cùng 1 ghế?"*
* **Trả lời**:
  * *"Tại `Plugins/Cinema.DataStore.SQL.Dapper/Repositories/BookingRepository.cs` (hàm `CreateBookingAsync`), hệ thống mở một `SqlConnection` và bắt đầu một `SqlTransaction`."*
  * *"Đầu tiên ghi bản ghi Header vào bảng `Booking`. Sau đó lặp qua danh sách ghế để ghi từng vé con vào bảng `Ticket` trong cùng Transaction đó."*
  * *"Dưới CSDL SQL Server, bảng `Ticket` có ràng buộc Unique Index: `UQ_Ticket_Showtime_Seat (ShowtimeId, SeatId)`. Nếu 2 người cùng nhấn thanh toán ở cùng một mili-giây cho cùng một ghế, SQL Server sẽ phát sinh lỗi vi phạm Unique Key và toàn bộ Transaction sẽ bị Rollback ngay lập tức. Dữ liệu luôn đảm bảo tính toàn vẹn tuyệt đối (ACID)."*

### 4. Câu hỏi về Bảo mật & Phân quyền
* **Giáo viên hỏi**: *"Làm thế nào để chặn người dùng thường không vào được các trang Quản trị viên?"*
* **Trả lời**:
  * *"Ở tầng UI, tất cả các trang Admin trong `Cinema.Web/Components/Pages/Admin/` đều có directive `@attribute [Authorize(Roles = "Admin")]`."*
  * *"Tại file `Routes.razor`, thẻ `<AuthorizeRouteView>` sẽ kiểm tra Claims trong `AuthenticationState`. Nếu người dùng có role là Customer hoặc chưa đăng nhập, hệ thống sẽ rơi vào nhánh `<NotAuthorized>` và hiển thị màn hình cảnh báo không có quyền truy cập, không cho phép hiển thị bất kỳ thành phần nào của trang Admin."*
