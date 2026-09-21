# ĐỒ ÁN CAPSTONE: RẠP PHIM - CHỌN GHẾ & ĐẶT VÉ (CINEMA SEAT BOOKING)
**Học phần:** Lập trình ứng dụng Web  
**Trường:** Đại học Kinh tế - Đại học Huế (HUE)  
**Đề tài 14:** Rạp phim: Chọn ghế & Đặt vé — Cinema Seat Booking  
**Độ khó:** ★★★ (3 sao - Thang điểm 9 - 10)  
**Sinh viên thực hiện:** Nguyễn Viết Mẫn  
**Mã số sinh viên (MSSV):** 23K4080026  

---

## 1. Tổng Quan Kiến Trúc (Clean Architecture)
Dự án được cấu trúc nghiêm ngặt theo **Clean Architecture** gồm các tầng độc lập:
1. `Cinema.CoreBusiness`: Chứa Domain Entities (`Movie`, `Auditorium`, `Seat`, `Showtime`, `Booking`, `Ticket`), Domain Exceptions và Domain Rules. Hoàn toàn độc lập, 0 dependencies.
2. `Cinema.UseCases`: Chứa các tương tác nghiệp vụ khách hàng và admin, cùng các Plugin Interfaces (`IMovieRepository`, `IShowtimeRepository`, `ISeatRepository`, `IBookingRepository`, `ISeatHoldingStateStore`). Chỉ phụ thuộc vào `Cinema.CoreBusiness`.
3. `Plugins/Cinema.DataStore.SQL.Dapper`: Triển khai truy xuất CSDL SQL Server 2022 (`CinemaDB`) sử dụng Dapper thuần với câu lệnh có tham số (Parameterized Queries) và quản lý Transaction ACID cho cặp Header-Line (`Booking` & `Ticket`).
4. `Plugins/Cinema.StateStore.DI`: Triển khai State Store giữ ghế tạm thời (In-Memory Concurrency Store) hỗ trợ giữ ghế 10 phút, tránh xung đột.
5. `Cinema.Web`: Blazor Web App (Interactive Server Mode) với giao diện sáng hiện đại (Light Theme), Cookie Authentication, sơ đồ chọn ghế trực quan thời gian thực.
6. `Cinema.UnitTests`: Dự án xUnit kiểm thử tự động 100% cho 2 quy tắc nghiệp vụ cốt lõi (Luật 1 & Luật 2).

---

## 2. Hai Quy Tắc Nghiệp Vụ Cốt Lõi (Mandatory Rules)
- **LUẬT 1 (Tranh chấp ghế - Seat Conflict):**
  - Một ghế trong một suất chiếu chỉ được bán DUY NHẤT một lần.
  - Ngăn ngừa tình trạng 2 khách hàng đồng thời chọn hoặc thanh toán cùng lúc.
  - Cơ chế bảo vệ 2 lớp:
    1. Kiểm tra trạng thái ghế khả dụng tại `Cinema.CoreBusiness/Rules/SeatBookingRule.cs` và State Store.
    2. Ràng buộc toàn vẹn `UNIQUE(ShowtimeId, SeatId)` trong SQL Server kết hợp `IDbTransaction` trong `BookingRepository.cs` (tự động rollback nếu xảy ra vi phạm).
- **LUẬT 2 (Không chồng giờ - Showtime Non-Overlapping):**
  - Các suất chiếu cùng phòng chiếu không được phép chồng giờ.
  - Thời gian kết thúc tự động tính: `EndTime = StartTime + MovieDuration + 15 phút dọn dẹp vệ sinh phòng chiếu`.
  - Được kiểm tra và bảo đảm bởi `Cinema.CoreBusiness/Rules/ShowtimeScheduleRule.cs`.

---

## 3. Hướng Dẫn Cài Đặt & Chạy Ứng Dụng
1. **Khởi tạo CSDL (Chỉ cần chạy 1 lần nếu chưa tạo):**
   - Đảm bảo SQL Server đang chạy tại `localhost`.
   - File script CSDL: `Cinema.SchemaAndData.sql` (tự động tạo DB `CinemaDB` và nạp sẵn dữ liệu 5 phim, 2 phòng chiếu, 96 ghế, 12 suất chiếu, vé mẫu).
   - Lệnh chạy:
     ```powershell
     sqlcmd -S localhost -E -C -i Cinema.SchemaAndData.sql
     ```
2. **Khởi chạy ứng dụng:**
   ```bash
   dotnet run --project Cinema.Web/Cinema.Web.csproj --launch-profile http
   ```
   Hoặc:
   ```bash
   cd Cinema.Web
   dotnet run --launch-profile http
   ```
   Truy cập trình duyệt: `http://localhost:5284`
3. **Chạy Unit Tests (Kiểm thử 2 Luật nghiệp vụ):**
   ```bash
   dotnet test
   ```
4. **Tài khoản Quản trị viên (Admin):**
   - Tài khoản: `admin`
   - Mật khẩu: `admin123`
