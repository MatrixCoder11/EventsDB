using EventsDB.Data;
using EventsDB.Interfaces;
using EventsDB.Models;
using System.Security.Cryptography;
using System.Text;

namespace EventsDB.Services;

public class AuthService : IAuthService
{
    private readonly DatabaseContext _db;

    public AuthService(DatabaseContext db)
    {
        _db = db;
    }

    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public User? Login(string username, string password)
    {
        var user = _db.GetUserByUsername(username);
        if (user is null) return null;

        var hash = HashPassword(password);
        return hash == user.PasswordHash ? user : null;
    }

    public void Register(string username, string password, UserRole role = UserRole.Viewer)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            Role = role
        };
        _db.CreateUser(user);
    }
}