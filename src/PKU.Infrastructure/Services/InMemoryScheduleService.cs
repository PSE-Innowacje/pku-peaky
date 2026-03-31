using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;

namespace PKU.Infrastructure.Services;

public class InMemoryScheduleService : IScheduleService
{
    private readonly List<Schedule> _schedules =
    [
        new Schedule
        {
            FeeType = FeeType.OP,
            ContractorType = ContractorType.OSDp,
            Items =
            [
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationSubmit, Days = 10, DayType = DayType.BusinessDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationInvoice, Days = 15, DayType = DayType.CalendarDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationCorrection, Days = 20, DayType = DayType.BusinessDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationCorrectionInvoice, Days = 25, DayType = DayType.CalendarDay }
            ]
        },
        new Schedule
        {
            FeeType = FeeType.OZE,
            ContractorType = ContractorType.Wytworca,
            Items =
            [
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationSubmit, Days = 7, DayType = DayType.CalendarDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationInvoice, Days = 12, DayType = DayType.CalendarDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationCorrection, Days = 17, DayType = DayType.BusinessDay },
                new ScheduleItem { ItemType = ScheduleItemType.DeclarationCorrectionInvoice, Days = 22, DayType = DayType.CalendarDay }
            ]
        }
    ];

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
