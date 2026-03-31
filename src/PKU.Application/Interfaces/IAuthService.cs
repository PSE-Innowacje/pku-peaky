using PKU.Domain.Entities;

namespace PKU.Application.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
}
