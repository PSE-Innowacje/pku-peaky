using PKU.Domain.Enums;

namespace PKU.Domain.Entities;

/// <summary>
/// Pozycja terminarza definiująca termin (liczbę dni) i typ dnia.
/// </summary>
public class ScheduleItem
{
    public ScheduleItemType ItemType { get; set; }
    public int Days { get; set; }
    public DayType DayType { get; set; } = DayType.CalendarDay;
}
