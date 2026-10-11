using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cinema.Web.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public Task<bool> LoginAsync(string username, string password)
    {
        // Tài khoản Quản trị viên mặc định theo quy định đồ án
        if (username.Trim().ToLower() == "admin" && password == "admin123")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Quản trị viên"),
                new(ClaimTypes.Role, "Admin"),
                new("Username", "admin")
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task LogoutAsync()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        return Task.CompletedTask;
    }
}
