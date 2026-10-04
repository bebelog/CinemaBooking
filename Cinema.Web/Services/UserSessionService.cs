namespace Cinema.Web.Services;

public class UserSessionService
{
    public string SessionId { get; } = Guid.NewGuid().ToString("N");
}
