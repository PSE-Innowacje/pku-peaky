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
                FirstName = "Administrator",
                LastName = "Systemowy",
                Email = "admin@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Administrator,
                ContractorTypes = []
            },
            new User
            {
                Id = "2",
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "osdp@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorAbbreviation = "OSDp-JK",
                ContractorFullName = "Jan Kowalski Sp. z o.o.",
                ContractorShortName = "JK Sp.",
                KRS = "0000123456",
                NIP = "1234567890",
                HeadquartersAddress = "ul. Energetyczna 1, 00-001 Warszawa",
                ContractorCode = "KON-001",
                ContractorTypes = [ContractorType.OSDp],
                ContractNumber = "UP/2025/001",
                ContractStartDate = new DateTime(2025, 1, 1),
                ContractEndDate = new DateTime(2026, 12, 31)
            },
            new User
            {
                Id = "3",
                FirstName = "Anna",
                LastName = "Nowak",
                Email = "osdn@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorAbbreviation = "OSDn-AN",
                ContractorFullName = "Anna Nowak Energia S.A.",
                ContractorShortName = "AN Energia",
                KRS = "0000234567",
                NIP = "2345678901",
                HeadquartersAddress = "ul. Przesylowa 5, 00-002 Krakow",
                ContractorCode = "KON-002",
                ContractorTypes = [ContractorType.OSDn],
                ContractNumber = "UP/2025/002",
                ContractStartDate = new DateTime(2025, 3, 1),
                ContractEndDate = new DateTime(2027, 2, 28)
            },
            new User
            {
                Id = "4",
                FirstName = "Piotr",
                LastName = "Wisniewski",
                Email = "ok@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorAbbreviation = "OK-PW",
                ContractorFullName = "Piotr Wisniewski Odbiorca Koncowy",
                ContractorShortName = "PW Odbiorca",
                KRS = "0000345678",
                NIP = "3456789012",
                HeadquartersAddress = "ul. Odbiorcza 10, 00-003 Gdansk",
                ContractorCode = "KON-003",
                ContractorTypes = [ContractorType.OdbiorcaKoncowy],
                ContractNumber = "UP/2025/003",
                ContractStartDate = new DateTime(2025, 6, 1),
                ContractEndDate = new DateTime(2026, 5, 31)
            },
            new User
            {
                Id = "5",
                FirstName = "Maria",
                LastName = "Zielinska",
                Email = "wyt@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorAbbreviation = "WYT-MZ",
                ContractorFullName = "Maria Zielinska Wytwarzanie Sp. z o.o.",
                ContractorShortName = "MZ Wyt",
                KRS = "0000456789",
                NIP = "4567890123",
                HeadquartersAddress = "ul. Wytworcza 20, 00-004 Poznan",
                ContractorCode = "KON-004",
                ContractorTypes = [ContractorType.Wytworca],
                ContractNumber = "UP/2025/004",
                ContractStartDate = new DateTime(2025, 1, 15),
                ContractEndDate = new DateTime(2027, 1, 14)
            },
            new User
            {
                Id = "6",
                FirstName = "Tomasz",
                LastName = "Lewandowski",
                Email = "mag@pku.pl",
                PasswordHash = hash,
                Role = UserRole.Kontrahent,
                ContractorAbbreviation = "MAG-TL",
                ContractorFullName = "Tomasz Lewandowski Magazyn Energii S.A.",
                ContractorShortName = "TL Magazyn",
                KRS = "0000567890",
                NIP = "5678901234",
                HeadquartersAddress = "ul. Magazynowa 30, 00-005 Wroclaw",
                ContractorCode = "KON-005",
                ContractorTypes = [ContractorType.Magazyn],
                ContractNumber = "UP/2025/005",
                ContractStartDate = new DateTime(2025, 4, 1),
                ContractEndDate = new DateTime(2026, 3, 31)
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
