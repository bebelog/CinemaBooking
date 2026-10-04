using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Showtimes;

public class DeleteShowtimeUseCase : IDeleteShowtimeUseCase
{
    private readonly IShowtimeRepository _showtimeRepo;

    public DeleteShowtimeUseCase(IShowtimeRepository showtimeRepo)
    {
        _showtimeRepo = showtimeRepo;
    }

    public async Task<bool> ExecuteAsync(int showtimeId)
    {
        return await _showtimeRepo.DeleteShowtimeAsync(showtimeId);
    }
}
