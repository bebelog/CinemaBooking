using Cinema.CoreBusiness.Models;
using Cinema.CoreBusiness.Rules;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Showtimes;

/// <summary>
/// Thêm suất chiếu mới kèm áp dụng LUẬT 2: Suất chiếu cùng phòng không được chồng giờ
/// EndTime = StartTime + MovieDuration + 15 phút dọn phòng
/// </summary>
public class CreateShowtimeUseCase : ICreateShowtimeUseCase
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;

    public CreateShowtimeUseCase(IShowtimeRepository showtimeRepository, IMovieRepository movieRepository)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
    }

    public async Task<int> ExecuteAsync(Showtime newShowtime)
    {
        var movie = await _movieRepository.GetMovieByIdAsync(newShowtime.MovieId);
        if (movie == null)
            throw new InvalidOperationException("Phim được chọn không tồn tại trong hệ thống.");

        // Tự động tính EndTime = StartTime + Duration + 15 phút dọn dẹp vệ sinh phòng chiếu
        newShowtime.EndTime = ShowtimeScheduleRule.CalculateEndTime(newShowtime.StartTime, movie.DurationMinutes);

        // Lấy danh sách suất chiếu trong phạm vi lân cận tại phòng chiếu (tránh bỏ sót suất vắt qua nửa đêm)
        DateTime searchStart = newShowtime.StartTime.AddHours(-6);
        DateTime searchEnd = newShowtime.EndTime.AddHours(6);

        var existingShowtimes = await _showtimeRepository.GetShowtimesByAuditoriumAndDateRangeAsync(
            newShowtime.AuditoriumId,
            searchStart,
            searchEnd);

        // LUẬT 2: Validate không chồng giờ
        ShowtimeScheduleRule.ValidateNoOverlap(newShowtime, existingShowtimes);

        return await _showtimeRepository.AddShowtimeAsync(newShowtime);
    }
}
