using System.Security.Cryptography;
using System.Text;
using Cinema.CoreBusiness.Models;
using Cinema.UseCases.PluginInterfaces.DataStore;
using Dapper;

namespace Cinema.DataStore.SQL.Dapper.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _factory;

    public UserRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("Cinema_Salt_Starlight_" + password));
        return Convert.ToBase64String(bytes);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT * FROM dbo.[User] WHERE LOWER(Username) = LOWER(@Username)";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username.Trim() });
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT * FROM dbo.[User] WHERE LOWER(Email) = LOWER(@Email)";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email.Trim() });
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var connection = _factory.CreateConnection();
        const string sql = "SELECT * FROM dbo.[User] WHERE UserId = @UserId";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<int> CreateUserAsync(User user, string plainPassword)
    {
        using var connection = _factory.CreateConnection();
        user.PasswordHash = HashPassword(plainPassword);

        const string sql = @"
            INSERT INTO dbo.[User] (Username, PasswordHash, FullName, Email, PhoneNumber, Role, CreatedAt)
            VALUES (@Username, @PasswordHash, @FullName, @Email, @PhoneNumber, @Role, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        int id = await connection.ExecuteScalarAsync<int>(sql, user);
        user.UserId = id;
        return id;
    }

    public async Task<User?> ValidateUserCredentialsAsync(string usernameOrEmail, string plainPassword)
    {
        using var connection = _factory.CreateConnection();
        const string sql = @"
            SELECT * FROM dbo.[User]
            WHERE LOWER(Username) = LOWER(@Keyword) OR LOWER(Email) = LOWER(@Keyword)";

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Keyword = usernameOrEmail.Trim() });
        if (user == null) return null;

        // Cho phép đăng nhập bằng cả mật khẩu plain (tài khoản seed admin123, khach123) hoặc hash SHA-256
        string hashed = HashPassword(plainPassword);
        if (user.PasswordHash == plainPassword || user.PasswordHash == hashed)
        {
            return user;
        }

        return null;
    }
}
