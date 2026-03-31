using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class InMemoryScheduleService : IScheduleService
{
    private readonly List<Schedule> _schedules = [];

    public Task<IEnumerable<Schedule>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Schedule>>(_schedules);

    public Task<Schedule?> GetByIdAsync(string id) =>
        Task.FromResult(_schedules.FirstOrDefault(s => s.Id == id));

    public Task CreateAsync(Schedule schedule)
    {
        _schedules.Add(schedule);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Schedule schedule)
    {
        var index = _schedules.FindIndex(s => s.Id == schedule.Id);
        if (index >= 0) _schedules[index] = schedule;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _schedules.RemoveAll(s => s.Id == id);
        return Task.CompletedTask;
    }
}
