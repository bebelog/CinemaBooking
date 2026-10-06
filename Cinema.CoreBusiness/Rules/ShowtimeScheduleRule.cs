using Cinema.CoreBusiness.Exceptions;
using Cinema.CoreBusiness.Models;

namespace Cinema.CoreBusiness.Rules;

/// <summary>
/// LUẬT 2: Không chồng giờ suất chiếu (Showtime Non-overlapping Rule)
/// - Các suất chiếu trong cùng một phòng chiếu không được phép chồng chéo thời gian.
/// - Giữa 2 suất chiếu cần ít nhất 15 phút dọn dẹp vệ sinh phòng chiếu (Clean-up Buffer).
/// </summary>
public static class ShowtimeScheduleRule
{
    public const int CleaningBufferMinutes = 15;

    public static DateTime CalculateEndTime(DateTime startTime, int movieDurationMinutes)
    {
        return startTime.AddMinutes(movieDurationMinutes + CleaningBufferMinutes);
    }

    public static void ValidateNoOverlap(
        Showtime newShowtime, 
        IEnumerable<Showtime> existingShowtimesInSameAuditorium)
    {
        if (newShowtime.EndTime <= newShowtime.StartTime)
        {
            throw new ShowtimeOverlapException("Thời gian kết thúc suất chiếu phải sau thời gian bắt đầu!");
        }

        foreach (var existing in existingShowtimesInSameAuditorium)
        {
            if (existing.ShowtimeId == newShowtime.ShowtimeId) continue;

            bool isOverlap = (newShowtime.StartTime < existing.EndTime) && (newShowtime.EndTime > existing.StartTime);
            if (isOverlap)
            {
                throw new ShowtimeOverlapException(
                    $"Suất chiếu bị trùng lịch với suất chiếu ID #{existing.ShowtimeId} " +
                    $"({existing.StartTime:HH:mm dd/MM/yyyy} - {existing.EndTime:HH:mm dd/MM/yyyy}) " +
                    $"tại phòng chiếu này! Giữa các suất chiếu cần tối thiểu {CleaningBufferMinutes} phút dọn phòng.");
            }
        }
    }
}
