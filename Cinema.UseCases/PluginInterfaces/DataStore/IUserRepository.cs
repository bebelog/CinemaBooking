using Cinema.CoreBusiness.Models;

namespace Cinema.UseCases.PluginInterfaces.DataStore;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<int> CreateUserAsync(User user, string plainPassword);
    Task<User?> ValidateUserCredentialsAsync(string usernameOrEmail, string plainPassword);
}
