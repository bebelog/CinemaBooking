using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Models;
using Cinema.CoreBusiness.Rules;
using Xunit;

namespace Cinema.UnitTests;

/// <summary>
/// Kiểm thử LUẬT 2: Quy tắc xếp lịch chiếu & Chống chồng giờ (Showtime Schedule Rule)
/// Bao gồm:
/// - Tính toán giờ kết thúc phim & giờ dọn rạp 15 phút
/// - Chống chồng chéo suất chiếu trong cùng phòng
/// - Chặn xếp lịch trong quá khứ
/// - Kiểm tra khung giờ mở cửa rạp (08:00 - 23:30)
/// </summary>
public class Rule2_ShowtimeOverlapTests
{
    // --- TẦNG 1: TÍNH TOÁN THỜI LƯỢNG & 15 PHÚT DỌN PHÒNG ---

    [Fact]
    public void CalculateEndTime_AddsDurationPlus15MinutesBuffer()
    {
        // Arrange: Phim 120 phút, bắt đầu lúc 09:00
        var startTime = new DateTime(2026, 10, 1, 9, 0, 0);
        int durationMinutes = 120;

        // Act: 120 + 15 = 135 phút -> 11:15 (Phòng chiếu sẵn sàng)
        var endTime = ShowtimeScheduleRule.CalculateEndTime(startTime, durationMinutes);

        // Assert
        Assert.Equal(new DateTime(2026, 10, 1, 11, 15, 0), endTime);
    }

    [Fact]
    public void CalculateTimes_DistinguishesMovieEndTimeAndAuditoriumAvailableTime()
    {
        // Arrange: Phim 100 phút bắt đầu lúc 14:00
        var startTime = new DateTime(2026, 10, 1, 14, 0, 0);
        int durationMinutes = 100;

        // Act
        var movieEndTime = ShowtimeScheduleRule.CalculateMovieEndTime(startTime, durationMinutes);
        var readyTime = ShowtimeScheduleRule.CalculateAuditoriumAvailableTime(startTime, durationMinutes);

        // Assert: Giờ hết phim in lên vé là 15:40; Giờ dọn xong phòng là 15:55
        Assert.Equal(new DateTime(2026, 10, 1, 15, 40, 0), movieEndTime);
        Assert.Equal(new DateTime(2026, 10, 1, 15, 55, 0), readyTime);
        Assert.Equal(15, (readyTime - movieEndTime).TotalMinutes);
    }

    // --- TẦNG 2: CHỐNG CHỒNG CHÉO THỜI GIAN TRONG CÙNG PHÒNG CHIẾU ---

    [Fact]
    public void ValidateNoOverlap_WhenOverlapsExistingShowtime_ThrowsShowtimeOverlapException()
    {
        // Arrange: Suất hiện có 09:00 - 11:15 tại phòng 1
        var existing = new List<Showtime>
        {
            new()
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                StartTime = new DateTime(2026, 10, 1, 9, 0, 0),
                EndTime = new DateTime(2026, 10, 1, 11, 15, 0)
            }
        };

        // Suất mới định xếp lúc 10:00 - 12:00 (chồng giờ trực tiếp)
        var newShowtime = new Showtime
        {
            ShowtimeId = 2,
            AuditoriumId = 1,
            StartTime = new DateTime(2026, 10, 1, 10, 0, 0),
            EndTime = new DateTime(2026, 10, 1, 12, 0, 0)
        };

        // Act & Assert
        var ex = Assert.Throws<ShowtimeOverlapException>(() =>
            ShowtimeScheduleRule.ValidateNoOverlap(newShowtime, existing));

        Assert.Contains("trùng lịch", ex.Message);
    }

    [Fact]
    public void ValidateNoOverlap_WhenViolates15MinutesCleaningBuffer_ThrowsShowtimeOverlapException()
    {
        // Arrange: Suất hiện có: Phim kết thúc lúc 11:00, cộng 15 phút dọn rạp -> EndTime = 11:15
        var existing = new List<Showtime>
        {
            new()
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                StartTime = new DateTime(2026, 10, 1, 9, 0, 0),
                EndTime = new DateTime(2026, 10, 1, 11, 15, 0)
            }
        };

        // Suất mới định bắt đầu lúc 11:05 (chưa dọn rạp xong, vi phạm buffer)
        var newShowtime = new Showtime
        {
            ShowtimeId = 2,
            AuditoriumId = 1,
            StartTime = new DateTime(2026, 10, 1, 11, 5, 0),
            EndTime = new DateTime(2026, 10, 1, 13, 0, 0)
        };

        // Act & Assert
        var ex = Assert.Throws<ShowtimeOverlapException>(() =>
            ShowtimeScheduleRule.ValidateNoOverlap(newShowtime, existing));

        Assert.Contains("trùng lịch", ex.Message);
    }

    [Fact]
    public void ValidateNoOverlap_WhenScheduledProperlyAfterBuffer_DoesNotThrow()
    {
        // Arrange: Suất hiện có 09:00 - 11:15 tại phòng 1
        var existing = new List<Showtime>
        {
            new()
            {
                ShowtimeId = 1,
                AuditoriumId = 1,
                StartTime = new DateTime(2026, 10, 1, 9, 0, 0),
                EndTime = new DateTime(2026, 10, 1, 11, 15, 0)
            }
        };

        // Suất mới bắt đầu lúc 11:20 (sau khi đã dọn phòng xong)
        var newShowtime = new Showtime
        {
            ShowtimeId = 2,
            AuditoriumId = 1,
            StartTime = new DateTime(2026, 10, 1, 11, 20, 0),
            EndTime = new DateTime(2026, 10, 1, 13, 30, 0)
        };

        // Act & Assert (Không có lỗi)
        ShowtimeScheduleRule.ValidateNoOverlap(newShowtime, existing);
    }

    // --- TẦNG 3: KIỂM THỬ THỜI ĐIỂM QUÁ KHỨ & KHUNG GIỜ HOẠT ĐỘNG ---

    [Fact]
    public void ValidateNotInPast_WhenStartTimeInPast_ThrowsShowtimeOverlapException()
    {
        // Arrange: Thời điểm hiện tại là 14:00, suất định tạo là 10:00 sáng cùng ngày (quá khứ)
        var now = new DateTime(2026, 10, 15, 14, 0, 0);
        var pastStartTime = new DateTime(2026, 10, 15, 10, 0, 0);

        // Act & Assert
        var ex = Assert.Throws<ShowtimeOverlapException>(() =>
            ShowtimeScheduleRule.ValidateNotInPast(pastStartTime, now));

        Assert.Contains("quá khứ", ex.Message);
    }

    [Fact]
    public void ValidateNotInPast_WhenStartTimeInFuture_DoesNotThrow()
    {
        // Arrange: Suất định tạo là ngày mai lúc 10:00 (tương lai)
        var now = new DateTime(2026, 10, 15, 14, 0, 0);
        var futureStartTime = new DateTime(2026, 10, 16, 10, 0, 0);

        // Act & Assert: Phải pass hợp lệ
        ShowtimeScheduleRule.ValidateNotInPast(futureStartTime, now);
    }

    [Fact]
    public void ValidateOperatingHours_WhenEarlyMorning3AM_ThrowsShowtimeOverlapException()
    {
        // Arrange: Suất chiếu bắt đầu lúc 03:30 sáng (ngoài khung giờ rạp mở cửa)
        var earlyMorningTime = new DateTime(2026, 10, 16, 3, 30, 0);

        // Act & Assert
        var ex = Assert.Throws<ShowtimeOverlapException>(() =>
            ShowtimeScheduleRule.ValidateOperatingHours(earlyMorningTime));

        Assert.Contains("khung giờ mở cửa rạp", ex.Message);
    }

    [Fact]
    public void ValidateOperatingHours_WhenWithinBusinessHours_DoesNotThrow()
    {
        // Arrange: Suất chiếu lúc 19:30 tối (giờ vàng xem phim)
        var primeTime = new DateTime(2026, 10, 16, 19, 30, 0);

        // Act & Assert: Phải pass hợp lệ
        ShowtimeScheduleRule.ValidateOperatingHours(primeTime);
    }
}
