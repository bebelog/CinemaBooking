using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.AI;

public class SemanticMovieSearchUseCase : ISemanticMovieSearchUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowtimeRepository _showtimeRepository;

    public SemanticMovieSearchUseCase(
        IMovieRepository movieRepository,
        IShowtimeRepository showtimeRepository)
    {
        _movieRepository = movieRepository;
        _showtimeRepository = showtimeRepository;
    }

    public async Task<List<AiMovieRecommendationDto>> ExecuteAsync(string naturalLanguageQuery, DateTime? preferredDate = null)
    {
        if (string.IsNullOrWhiteSpace(naturalLanguageQuery))
        {
            return new List<AiMovieRecommendationDto>();
        }

        var normalizedQuery = NormalizeText(naturalLanguageQuery);
        var activeMovies = (await _movieRepository.GetAllMoviesAsync(activeOnly: true)).ToList();
        var allShowtimes = (await _showtimeRepository.GetAllUpcomingShowtimesAsync()).ToList();

        // 1. Phân tích khung giờ mong muốn từ câu hỏi
        var (timeFilterStart, timeFilterEnd, timeReason) = DetectTimePreference(normalizedQuery);
        var targetDate = preferredDate ?? DetectTargetDate(normalizedQuery);

        var recommendations = new List<AiMovieRecommendationDto>();

        foreach (var movie in activeMovies)
        {
            var (score, matchReasons) = CalculateSemanticScore(movie, normalizedQuery);

            if (score > 15.0) // Ngưỡng phù hợp tối thiểu
            {
                // Lọc suất chiếu phù hợp cho phim này
                var movieShowtimes = allShowtimes
                    .Where(s => s.MovieId == movie.MovieId)
                    .Where(s => s.StartTime.Date == targetDate.Date)
                    .Where(s => s.StartTime.TimeOfDay >= timeFilterStart && s.StartTime.TimeOfDay <= timeFilterEnd)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                // Nếu không có suất trong khung giờ ưu tiên, lấy các suất cùng ngày
                if (!movieShowtimes.Any())
                {
                    movieShowtimes = allShowtimes
                        .Where(s => s.MovieId == movie.MovieId)
                        .Where(s => s.StartTime.Date == targetDate.Date)
                        .OrderBy(s => s.StartTime)
                        .ToList();
                }

                // Ghép lý do gợi ý hoàn chỉnh
                var reasonBuilder = new StringBuilder();
                if (matchReasons.Any())
                {
                    reasonBuilder.Append(string.Join(". ", matchReasons));
                }
                else
                {
                    reasonBuilder.Append($"Nội dung phù hợp với mô tả: \"{naturalLanguageQuery}\"");
                }

                if (!string.IsNullOrEmpty(timeReason) && movieShowtimes.Any())
                {
                    reasonBuilder.Append($". {timeReason}");
                }

                recommendations.Add(new AiMovieRecommendationDto
                {
                    Movie = movie,
                    RelevanceScore = Math.Min(100.0, Math.Round(score, 1)),
                    MatchReason = reasonBuilder.ToString(),
                    SuggestedShowtimes = movieShowtimes
                });
            }
        }

        return recommendations.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    private (double Score, List<string> Reasons) CalculateSemanticScore(Movie movie, string query)
    {
        double score = 0;
        var reasons = new List<string>();

        var titleNorm = NormalizeText(movie.Title);
        var genreNorm = NormalizeText(movie.Genre);
        var descNorm = NormalizeText(movie.Description ?? "");
        var rating = (movie.AgeRating ?? "").ToUpperInvariant();

        // Topic 1: Gia đình / Trẻ em / Nhẹ nhàng
        bool isFamilyQuery = query.Contains("gia dinh") || query.Contains("tre em") || 
                             query.Contains("nhe nhang") || query.Contains("hoat hinh") ||
                             query.Contains("thieu nhi") || query.Contains("vui ve") ||
                             query.Contains("hai huoc");

        if (isFamilyQuery)
        {
            if (rating == "P" || rating == "K")
            {
                score += 45;
                reasons.Add($"Phim có nhãn {rating} (phù hợp mọi lứa tuổi / gia đình và trẻ em)");
            }
            else if (rating == "T18")
            {
                score -= 40; // Trừ điểm phim 18+ nếu tìm cho gia đình
            }

            if (genreNorm.Contains("hoat hinh") || genreNorm.Contains("gia dinh") || genreNorm.Contains("hai"))
            {
                score += 35;
                reasons.Add("Thể loại hoạt hình, gia đình vui vẻ, giàu cảm xúc");
            }

            if (descNorm.Contains("gia dinh") || descNorm.Contains("me") || descNorm.Contains("con") || descNorm.Contains("tre em"))
            {
                score += 20;
            }
        }

        // Topic 2: Hành động / Kịch tính / Bom tấn
        bool isActionQuery = query.Contains("hanh dong") || query.Contains("kich tinh") || 
                             query.Contains("bom tan") || query.Contains("chien dau") ||
                             query.Contains("vien tuong") || query.Contains("khoa hoc") ||
                             query.Contains("danh nhau") || query.Contains("quai vat");

        if (isActionQuery)
        {
            if (genreNorm.Contains("hanh dong") || genreNorm.Contains("vien tuong") || genreNorm.Contains("phieu luu"))
            {
                score += 45;
                reasons.Add("Thể loại hành động, viễn tưởng mãn nhãn kỹ xảo");
            }
            if (descNorm.Contains("danh nhau") || descNorm.Contains("doi dau") || descNorm.Contains("huy diet") || descNorm.Contains("titan"))
            {
                score += 30;
                reasons.Add("Các pha giao tranh nghẹt thở và kịch tính");
            }
        }

        // Topic 3: Tình cảm / Tâm lý / Lãng mạn
        bool isRomanceQuery = query.Contains("tinh cam") || query.Contains("tam ly") || 
                              query.Contains("lang man") || query.Contains("hen ho") ||
                              query.Contains("sau lang") || query.Contains("tinh yeu");

        if (isRomanceQuery)
        {
            if (genreNorm.Contains("tam ly") || genreNorm.Contains("tinh cam"))
            {
                score += 50;
                reasons.Add("Nội dung tâm lý tình cảm sâu sắc, chạm đến cảm xúc");
            }
            if (rating == "T18")
            {
                score += 15;
                reasons.Add("Phù hợp cho các cặp đôi trưởng thành");
            }
        }

        // Khớp từ khóa trực tiếp (Keyword overlap)
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Where(w => w.Length > 2)
                              .ToList();

        int matchedWords = 0;
        foreach (var word in queryWords)
        {
            if (titleNorm.Contains(word))
            {
                score += 25;
                matchedWords++;
            }
            else if (genreNorm.Contains(word))
            {
                score += 15;
                matchedWords++;
            }
            else if (descNorm.Contains(word))
            {
                score += 10;
                matchedWords++;
            }
        }

        if (matchedWords > 0 && !reasons.Any())
        {
            reasons.Add($"Khớp {matchedWords} đặc trưng ngữ nghĩa trong mô tả phim");
        }

        return (score, reasons);
    }

    private (TimeSpan Start, TimeSpan End, string Reason) DetectTimePreference(string query)
    {
        if (query.Contains("toi nay") || query.Contains("buoi toi") || query.Contains("toi"))
        {
            return (new TimeSpan(18, 0, 0), new TimeSpan(21, 30, 0), "Ưu tiên các suất chiếu buổi tối (18h00 - 21h30)");
        }
        if (query.Contains("dem") || query.Contains("muon") || query.Contains("khuya"))
        {
            return (new TimeSpan(21, 30, 0), new TimeSpan(23, 59, 59), "Ưu tiên các suất chiếu muộn sau 21h30");
        }
        if (query.Contains("chieu") || query.Contains("buoi chieu"))
        {
            return (new TimeSpan(13, 0, 0), new TimeSpan(18, 0, 0), "Ưu tiên các suất chiếu buổi chiều (13h00 - 18h00)");
        }
        if (query.Contains("sang") || query.Contains("buoi sang"))
        {
            return (new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), "Ưu tiên các suất chiếu buổi sáng (08h00 - 12h00)");
        }

        return (TimeSpan.Zero, new TimeSpan(23, 59, 59), string.Empty);
    }

    private DateTime DetectTargetDate(string query)
    {
        if (query.Contains("ngay mai") || query.Contains("mai"))
        {
            return DateTime.Today.AddDays(1);
        }
        return DateTime.Today;
    }

    private string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        text = text.ToLowerInvariant().Trim();
        // Bỏ dấu tiếng Việt để tìm kiếm ngữ nghĩa linh hoạt
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        string result = sb.ToString().Normalize(NormalizationForm.FormC);
        result = result.Replace('đ', 'd').Replace('Đ', 'd');
        return Regex.Replace(result, @"s+", " ");
    }
}
