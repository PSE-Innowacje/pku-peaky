using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;

namespace PKU.Infrastructure.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _users;

    public InMemoryUserService()
    {
        var hash = AuthService.HashPassword("admin123");
        _users =
        [
            new User
            {
                Id = "1",
                Name = "Administrator Systemowy",
                Email = "admin@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Administrator,
                ContractorType = ContractorType.None
            },
            new User
            {
                Id = "2",
                Name = "Jan Kowalski (OSDp)",
                Email = "osdp@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorType = ContractorType.OSDp
            },
            new User
            {
                Id = "3",
                Name = "Anna Nowak (OSDn)",
                Email = "osdn@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorType = ContractorType.OSDn
            },
            new User
            {
                Id = "4",
                Name = "Piotr Wisniewski (OK)",
                Email = "ok@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorType = ContractorType.OK
            },
            new User
            {
                Id = "5",
                Name = "Maria Zielinska (Wyt)",
                Email = "wyt@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorType = ContractorType.Wyt
            },
            new User
            {
                Id = "6",
                Name = "Tomasz Lewandowski (Mag)",
                Email = "mag@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorType = ContractorType.Mag
            }
        ];
    }

    public Task<IEnumerable<User>> GetAllAsync() =>
        Task.FromResult<IEnumerable<User>>(_users);

    public Task<User?> GetByIdAsync(string id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task CreateAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0) _users[index] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _users.RemoveAll(u => u.Id == id);
        return Task.CompletedTask;
    }
}
