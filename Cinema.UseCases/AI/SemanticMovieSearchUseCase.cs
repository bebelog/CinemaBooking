using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.DTOs;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.AI;

public class SemanticMovieSearchUseCase : ISemanticMovieSearchUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly HttpClient? _httpClient;
    private readonly string? _geminiApiKey;
    private readonly string _geminiModel;

    public SemanticMovieSearchUseCase(
        IMovieRepository movieRepository,
        IShowtimeRepository showtimeRepository,
        HttpClient? httpClient = null,
        string? geminiApiKey = null,
        string? geminiModel = null)
    {
        _movieRepository = movieRepository;
        _showtimeRepository = showtimeRepository;
        _httpClient = httpClient;
        _geminiApiKey = geminiApiKey;
        _geminiModel = string.IsNullOrWhiteSpace(geminiModel) ? "gemini-2.5-flash" : geminiModel;
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

        var (timeFilterStart, timeFilterEnd, timeReason) = DetectTimePreference(normalizedQuery);
        var targetDate = preferredDate ?? DetectTargetDate(normalizedQuery);

        // Gọi Google Gemini 2.5 Flash nếu có cấu hình
        if (!string.IsNullOrWhiteSpace(_geminiApiKey) && _httpClient != null)
        {
            try
            {
                var geminiResults = await TryCallGeminiAsync(naturalLanguageQuery, activeMovies, allShowtimes, targetDate, timeFilterStart, timeFilterEnd, timeReason);
                if (geminiResults != null && geminiResults.Any())
                {
                    return geminiResults;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gemini Fallback] Lỗi API: {ex.Message}. Chuyển sang Local Semantic Engine.");
            }
        }

        return RunLocalSemanticSearch(naturalLanguageQuery, normalizedQuery, activeMovies, allShowtimes, targetDate, timeFilterStart, timeFilterEnd, timeReason);
    }

    private async Task<List<AiMovieRecommendationDto>?> TryCallGeminiAsync(
        string userQuery, 
        List<Movie> movies, 
        List<Showtime> allShowtimes, 
        DateTime targetDate, 
        TimeSpan timeStart, 
        TimeSpan timeEnd, 
        string timeReason)
    {
        var moviesSummary = string.Join("\n", movies.Select(m => 
            $"- ID {m.MovieId}: \"{m.Title}\" (Thể loại: {m.Genre}, Độ tuổi: {m.AgeRating}, Thời lượng: {m.DurationMinutes} phút). Mô tả: {m.Description}"));

        var systemPrompt = "Bạn là Trợ lý AI điện ảnh thông minh của rạp CineStar Huế.\n" +
            "Danh sách các bộ phim đang chiếu tại rạp:\n" + moviesSummary + "\n\n" +
            "Câu hỏi người dùng: \"" + userQuery + "\"\n\n" +
            "Nhiệm vụ:\n" +
            "1. Hiểu ngữ nghĩa và cảm xúc trong câu hỏi để chọn các bộ phim phù hợp nhất.\n" +
            "2. Với mỗi phim được chọn, chấm điểm phù hợp (score từ 60 đến 99) và viết 1 câu giải thích lý do ngắn gọn, thân thiện bằng tiếng Việt.\n" +
            "3. Trả về ĐÚNG định dạng JSON thuần túy (không dùng markdown, không có chữ thừa), theo mẫu:\n" +
            "[{\"movieId\": 2, \"score\": 96, \"reason\": \"Phim hoạt hình nhẹ nhàng nhãn P rất phù hợp cho gia đình có trẻ nhỏ cùng xem vui vẻ.\"}]";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = systemPrompt }
                    }
                }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModel}:generateContent?key={_geminiApiKey}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        var response = await _httpClient!.PostAsync(url, jsonContent, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        
        var textResult = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(textResult)) return null;

        var jsonClean = CleanJson(textResult);
        using var jsonParsed = JsonDocument.Parse(jsonClean);

        var recommendations = new List<AiMovieRecommendationDto>();

        foreach (var item in jsonParsed.RootElement.EnumerateArray())
        {
            if (item.TryGetProperty("movieId", out var movieIdProp))
            {
                int movieId = movieIdProp.GetInt32();
                var movie = movies.FirstOrDefault(m => m.MovieId == movieId);
                if (movie != null)
                {
                    double score = item.TryGetProperty("score", out var s) ? s.GetDouble() : 90;
                    string reason = item.TryGetProperty("reason", out var r) ? r.GetString() ?? "" : "";

                    var movieShowtimes = allShowtimes
                        .Where(st => st.MovieId == movie.MovieId)
                        .Where(st => st.StartTime.Date == targetDate.Date)
                        .Where(st => st.StartTime.TimeOfDay >= timeStart && st.StartTime.TimeOfDay <= timeEnd)
                        .OrderBy(st => st.StartTime)
                        .ToList();

                    if (!movieShowtimes.Any())
                    {
                        movieShowtimes = allShowtimes
                            .Where(st => st.MovieId == movie.MovieId)
                            .Where(st => st.StartTime.Date == targetDate.Date)
                            .OrderBy(st => st.StartTime)
                            .ToList();
                    }

                    recommendations.Add(new AiMovieRecommendationDto
                    {
                        Movie = movie,
                        RelevanceScore = score,
                        MatchReason = string.IsNullOrWhiteSpace(timeReason) ? reason : $"{reason}. {timeReason}",
                        SuggestedShowtimes = movieShowtimes
                    });
                }
            }
        }

        return recommendations.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    private string CleanJson(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json")) text = text.Substring(7);
        else if (text.StartsWith("```")) text = text.Substring(3);
        if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
        return text.Trim();
    }

    private List<AiMovieRecommendationDto> RunLocalSemanticSearch(
        string originalQuery,
        string normalizedQuery, 
        List<Movie> activeMovies, 
        List<Showtime> allShowtimes, 
        DateTime targetDate, 
        TimeSpan timeFilterStart, 
        TimeSpan timeFilterEnd, 
        string timeReason)
    {
        var recommendations = new List<AiMovieRecommendationDto>();

        foreach (var movie in activeMovies)
        {
            var (score, matchReasons) = CalculateSemanticScore(movie, normalizedQuery);

            if (score > 15.0)
            {
                var movieShowtimes = allShowtimes
                    .Where(s => s.MovieId == movie.MovieId)
                    .Where(s => s.StartTime.Date == targetDate.Date)
                    .Where(s => s.StartTime.TimeOfDay >= timeFilterStart && s.StartTime.TimeOfDay <= timeFilterEnd)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                if (!movieShowtimes.Any())
                {
                    movieShowtimes = allShowtimes
                        .Where(s => s.MovieId == movie.MovieId)
                        .Where(s => s.StartTime.Date == targetDate.Date)
                        .OrderBy(s => s.StartTime)
                        .ToList();
                }

                var reasonBuilder = new StringBuilder();
                if (matchReasons.Any())
                {
                    reasonBuilder.Append(string.Join(". ", matchReasons));
                }
                else
                {
                    reasonBuilder.Append($"Nội dung phù hợp với mô tả: \"{originalQuery}\"");
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
                score -= 40;
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
