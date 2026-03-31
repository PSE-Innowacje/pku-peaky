using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _users = [];

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
