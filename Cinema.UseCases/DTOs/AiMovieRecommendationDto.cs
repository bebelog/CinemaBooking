using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.DTOs;

/// <summary>
/// DTO đại diện cho kết quả gợi ý phim và suất chiếu từ Trợ lý AI Semantic Search
/// </summary>
public class AiMovieRecommendationDto
{
    public Movie Movie { get; set; } = new();

    /// <summary>
    /// Điểm tương đồng ngữ nghĩa (0.0 đến 100.0%)
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Lý do phân tích thông minh từ AI (giải thích tại sao phim phù hợp)
    /// </summary>
    public string MatchReason { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các suất chiếu phù hợp được AI gợi ý
    /// </summary>
    public List<Showtime> SuggestedShowtimes { get; set; } = new();
}
