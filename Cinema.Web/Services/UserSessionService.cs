using Microsoft.AspNetCore.Http;

namespace Cinema.Web.Services;

public class UserSessionService
{
    private const string CookieName = "Cinema_UserSessionId";
    public string SessionId { get; private set; }

    public UserSessionService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        string? existingId = null;

        if (httpContext != null && httpContext.Request.Cookies.TryGetValue(CookieName, out var cookieVal) && !string.IsNullOrWhiteSpace(cookieVal))
        {
            existingId = cookieVal;
        }

        if (string.IsNullOrWhiteSpace(existingId))
        {
            existingId = "user_" + Guid.NewGuid().ToString("N");
            if (httpContext != null && !httpContext.Response.HasStarted)
            {
                httpContext.Response.Cookies.Append(CookieName, existingId, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    IsEssential = true
                });
            }
        }

        SessionId = existingId;
    }

    public void SetSessionId(string newId)
    {
        if (!string.IsNullOrWhiteSpace(newId))
        {
            SessionId = newId;
        }
    }
}
