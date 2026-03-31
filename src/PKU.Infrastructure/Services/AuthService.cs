using System.Security.Cryptography;
using System.Text;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;

    public AuthService(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var users = await _userService.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);

        if (user is null)
            return null;

        var hash = HashPassword(password);
        return user.PasswordHash == hash ? user : null;
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
