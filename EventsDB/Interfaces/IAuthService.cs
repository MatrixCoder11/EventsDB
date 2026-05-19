using EventsDB.Models;

namespace EventsDB.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    User? Login(string username, string password);
    void Register(string username, string password, UserRole role = UserRole.Viewer);
}