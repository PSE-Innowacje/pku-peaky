using PKU.Domain.Enums;

namespace PKU.Domain.Entities;

/// <summary>
/// Terminarz miesięczny przypisany do typu opłaty i typu kontrahenta.
/// Definiuje terminy (w dniach od początku miesiąca) dla poszczególnych czynności rozliczeniowych.
/// </summary>
public class Schedule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public FeeType FeeType { get; set; }
    public ContractorType ContractorType { get; set; }
    public List<ScheduleItem> Items { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
