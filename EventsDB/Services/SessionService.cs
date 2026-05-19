using EventsDB.Models;

namespace EventsDB.Services;

public static class SessionService
{
    public static User? CurrentUser { get; private set; }

    public static bool IsAdmin => CurrentUser?.Role == "Admin";

    public static void Login(User user)
    {
        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}