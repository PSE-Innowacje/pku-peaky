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
    private int? _filterSubmitMin;
    private int? _filterSubmitMax;
    private int? _filterInvoiceMin;
    private int? _filterInvoiceMax;
    private int? _filterCorrectionMin;
    private int? _filterCorrectionMax;
    private int? _filterCorrInvoiceMin;
    private int? _filterCorrInvoiceMax;

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

        q = FilterByItemDays(q, ScheduleItemType.DeclarationSubmit, _filterSubmitMin, _filterSubmitMax);
        q = FilterByItemDays(q, ScheduleItemType.DeclarationInvoice, _filterInvoiceMin, _filterInvoiceMax);
        q = FilterByItemDays(q, ScheduleItemType.DeclarationCorrection, _filterCorrectionMin, _filterCorrectionMax);
        q = FilterByItemDays(q, ScheduleItemType.DeclarationCorrectionInvoice, _filterCorrInvoiceMin, _filterCorrInvoiceMax);

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

    private static IEnumerable<Schedule> FilterByItemDays(
        IEnumerable<Schedule> source, ScheduleItemType itemType, int? min, int? max)
    {
        if (!min.HasValue && !max.HasValue) return source;
        return source.Where(s =>
        {
            var item = s.Items.FirstOrDefault(i => i.ItemType == itemType);
            if (item is null) return false;
            if (min.HasValue && item.Days < min.Value) return false;
            if (max.HasValue && item.Days > max.Value) return false;
            return true;
        });
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
