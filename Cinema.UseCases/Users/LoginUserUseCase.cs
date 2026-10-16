using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;

namespace Cinema.UseCases.Users;

public interface ILoginUserUseCase
{
    Task<User?> ExecuteAsync(string usernameOrEmail, string plainPassword);
}

public class LoginUserUseCase : ILoginUserUseCase
{
    private readonly IUserRepository _userRepo;

    public LoginUserUseCase(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<User?> ExecuteAsync(string usernameOrEmail, string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(plainPassword))
            return null;

        return await _userRepo.ValidateUserCredentialsAsync(usernameOrEmail.Trim(), plainPassword);
    }
}
