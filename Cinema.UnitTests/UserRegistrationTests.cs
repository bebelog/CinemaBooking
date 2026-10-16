using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Cinema.UseCases.Users;
using Xunit;

namespace Cinema.UnitTests;

public class UserRegistrationTests
{
    private class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users = new()
        {
            new() { UserId = 1, Username = "admin", FullName = "Admin", Email = "admin@cinema.vn", Role = "Admin" },
            new() { UserId = 2, Username = "existinguser", FullName = "Existing", Email = "existing@gmail.com", Role = "Customer" }
        };

        public Task<User?> GetUserByUsernameAsync(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<User?> GetUserByIdAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            return Task.FromResult(user);
        }

        public Task<int> CreateUserAsync(User user, string plainPassword)
        {
            user.UserId = _users.Count + 1;
            _users.Add(user);
            return Task.FromResult(user.UserId);
        }

        public Task<User?> ValidateUserCredentialsAsync(string usernameOrEmail, string plainPassword)
        {
            return Task.FromResult<User?>(null);
        }
    }

    [Fact]
    public async Task RegisterUser_WhenValidInput_CreatesCustomerUserSuccessfully()
    {
        // Arrange
        var fakeRepo = new FakeUserRepository();
        var useCase = new RegisterUserUseCase(fakeRepo);

        // Act
        var createdUser = await useCase.ExecuteAsync(
            "mannguyen",
            "password123",
            "Nguyễn Viết Mẫn",
            "man.nguyen@example.com",
            "0905123456");

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal("mannguyen", createdUser.Username);
        Assert.Equal("Customer", createdUser.Role);
        Assert.Equal("Nguyễn Viết Mẫn", createdUser.FullName);
        Assert.True(createdUser.UserId > 0);
    }

    [Fact]
    public async Task RegisterUser_WhenUsernameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var fakeRepo = new FakeUserRepository();
        var useCase = new RegisterUserUseCase(fakeRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("existinguser", "secret123", "Người Khác", "new@gmail.com", null));

        Assert.Contains("đã được sử dụng", ex.Message);
    }

    [Fact]
    public async Task RegisterUser_WhenUsernameTooShort_ThrowsArgumentException()
    {
        // Arrange
        var fakeRepo = new FakeUserRepository();
        var useCase = new RegisterUserUseCase(fakeRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync("ab", "secret123", "Nguyễn An", "an@gmail.com", null));

        Assert.Contains("ít nhất 3 ký tự", ex.Message);
    }

    [Fact]
    public async Task RegisterUser_WhenPasswordTooShort_ThrowsArgumentException()
    {
        // Arrange
        var fakeRepo = new FakeUserRepository();
        var useCase = new RegisterUserUseCase(fakeRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync("validuser", "123", "Nguyễn An", "an@gmail.com", null));

        Assert.Contains("ít nhất 6 ký tự", ex.Message);
    }
}
