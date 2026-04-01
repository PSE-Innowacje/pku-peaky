using Microsoft.AspNetCore.Components;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;

namespace PKU.Web.Components.Pages.Admin;

public partial class Schedules
{
    [Inject] private IScheduleService ScheduleService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<Schedule>? _schedules;
    private IEnumerable<Schedule>? _filtered;
    private string? _confirmDeleteId;

    // Filters
    private string _filterFeeType = "";
    private string _filterContractorType = "";
    private string _filterActive = "";

    protected override async Task OnInitializedAsync()
    {
        _schedules = await ScheduleService.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _schedules?.AsEnumerable() ?? [];

        if (!string.IsNullOrWhiteSpace(_filterFeeType) && Enum.TryParse<FeeType>(_filterFeeType, out var ft))
            q = q.Where(s => s.FeeType == ft);

        if (!string.IsNullOrWhiteSpace(_filterContractorType) && Enum.TryParse<ContractorType>(_filterContractorType, out var ct))
            q = q.Where(s => s.ContractorType == ct);

        if (_filterActive == "true")
            q = q.Where(s => s.IsActive);
        else if (_filterActive == "false")
            q = q.Where(s => !s.IsActive);

        _filtered = q.OrderBy(s => s.FeeType).ThenBy(s => s.ContractorType).ToList();
    }

    private void AddSchedule()
    {
        Navigation.NavigateTo("/admin/schedules/new");
    }

    private void EditSchedule(string id)
    {
        Navigation.NavigateTo($"/admin/schedules/{id}");
    }

    private void DeleteSchedule(string id)
    {
        _confirmDeleteId = id;
    }

    private void CancelDelete()
    {
        _confirmDeleteId = null;
    }

    private async Task ConfirmDelete()
    {
        if (_confirmDeleteId is not null)
        {
            await ScheduleService.DeleteAsync(_confirmDeleteId);
            _confirmDeleteId = null;
            _schedules = await ScheduleService.GetAllAsync();
            ApplyFilter();
        }
    }

    internal static string FormatItem(Schedule schedule, ScheduleItemType itemType)
    {
        var item = schedule.Items.FirstOrDefault(i => i.ItemType == itemType);
        if (item is null) return "-";
        var dayLabel = item.DayType == DayType.CalendarDay ? "kal." : "rob.";
        return $"{item.Days} dni {dayLabel}";
    }

    internal static string GetContractorTypeLabel(ContractorType type) => type switch
    {
        ContractorType.OSDp => "OSDp",
        ContractorType.OSDn => "OSDn",
        ContractorType.Wytworca => "Wytworca",
        ContractorType.Magazyn => "Magazyn",
        ContractorType.OdbiorcaKoncowy => "Odbiorca Koncowy",
        _ => type.ToString()
    };
}
