using Microsoft.AspNetCore.Components;
using PKU.Application.Interfaces;
using PKU.Domain.Enums;

namespace PKU.Web.Components.Pages.Admin;

public partial class Users
{
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<PKU.Domain.Entities.User>? _users;
    private IEnumerable<PKU.Domain.Entities.User>? _filtered;
    private PKU.Domain.Entities.User? _confirmDeleteUser;

    private string _filterName = "";
    private string _filterEmail = "";
    private string _filterRole = "";
    private string _filterContractorType = "";
    private string _filterActive = "";

    protected override async Task OnInitializedAsync()
    {
        _users = await UserService.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _users?.AsEnumerable() ?? [];

        if (!string.IsNullOrWhiteSpace(_filterName))
            q = q.Where(u => u.FullName.Contains(_filterName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(_filterEmail))
            q = q.Where(u => u.Email.Contains(_filterEmail, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(_filterRole))
            q = q.Where(u => u.Role.ToString() == _filterRole);

        if (!string.IsNullOrWhiteSpace(_filterContractorType) && Enum.TryParse<ContractorType>(_filterContractorType, out var ct))
            q = q.Where(u => u.ContractorTypes.Contains(ct));

        if (_filterActive == "true")
            q = q.Where(u => u.IsActive);
        else if (_filterActive == "false")
            q = q.Where(u => !u.IsActive);

        _filtered = q.OrderBy(u => u.Email).ToList();
    }

    private void RequestDelete(PKU.Domain.Entities.User user)
    {
        _confirmDeleteUser = user;
    }

    private async Task ConfirmDelete()
    {
        if (_confirmDeleteUser is not null)
        {
            await UserService.DeleteAsync(_confirmDeleteUser.Id);
            _confirmDeleteUser = null;
            _users = await UserService.GetAllAsync();
            ApplyFilter();
        }
    }

    private void CancelDelete()
    {
        _confirmDeleteUser = null;
    }
}
