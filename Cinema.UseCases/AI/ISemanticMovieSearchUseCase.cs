using Cinema.UseCases.DTOs;

namespace Cinema.UseCases.AI;

/// <summary>
/// UseCase tìm kiếm phim theo ngữ nghĩa tự nhiên (Semantic Search) và Trợ lý gợi ý suất chiếu
/// </summary>
public interface ISemanticMovieSearchUseCase
{
    Task<List<AiMovieRecommendationDto>> ExecuteAsync(string naturalLanguageQuery, DateTime? preferredDate = null);
}
