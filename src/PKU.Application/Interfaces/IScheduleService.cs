using PKU.Domain.Entities;

namespace PKU.Application.Interfaces;

public interface IScheduleService
{
    Task<IEnumerable<Schedule>> GetAllAsync();
    Task<Schedule?> GetByIdAsync(string id);
    Task CreateAsync(Schedule schedule);
    Task UpdateAsync(Schedule schedule);
    Task DeleteAsync(string id);
}
