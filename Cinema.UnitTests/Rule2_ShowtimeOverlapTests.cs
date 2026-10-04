using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Models;
using Cinema.CoreBusiness.Rules;
using Xunit;

namespace Cinema.UnitTests;

/// <summary>
/// Kiểm thử LUẬT 2: Suất chiếu cùng phòng không được chồng giờ
/// EndTime = StartTime + Duration + 15 phút dọn phòng
/// </summary>
public class Rule2_ShowtimeOverlapTests
{
    [Fact]
    public void CalculateEndTime_AddsDurationPlus15MinutesBuffer()
    {
        // Arrange: Phim 120 phút, bắt đầu lúc 09:00
        var startTime = new DateTime(2026, 10, 1, 9, 0, 0);
        int durationMinutes = 120;

        // Act: 120 + 15 = 135 phút -> 11:15
        var endTime = ShowtimeScheduleRule.CalculateEndTime(startTime, durationMinutes);

        // Assert
        Assert.Equal(new DateTime(2026, 10, 1, 11, 15, 0), endTime);
    }

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

        // Suất mới định xếp lúc 10:00 - 12:00 (chồng giờ)
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
}
