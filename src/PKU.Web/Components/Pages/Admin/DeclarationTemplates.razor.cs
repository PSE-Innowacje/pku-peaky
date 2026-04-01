using Microsoft.AspNetCore.Components;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;

namespace PKU.Web.Components.Pages.Admin;

public partial class DeclarationTemplates
{
    [Inject] private IDeclarationTemplateService TemplateService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<DeclarationTemplate>? _templates;
    private IEnumerable<DeclarationTemplate>? _filtered;
    private DeclarationTemplate _formTemplate = new();
    private DeclarationTemplate? _editingTemplate;
    private bool _showForm;
    private string? _message;
    private bool _isError;

    // Filters
    private string _filterName = "";
    private string _filterFeeType = "";
    private string _filterContractorType = "";
    private string _filterComment = "";
    private string _filterActive = "";
    private int? _filterFieldsMin;
    private int? _filterFieldsMax;

    protected override async Task OnInitializedAsync()
    {
        await LoadTemplates();
    }

    private async Task LoadTemplates()
    {
        _templates = await TemplateService.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _templates?.AsEnumerable() ?? [];

        if (!string.IsNullOrWhiteSpace(_filterName))
            q = q.Where(t => t.Name.Contains(_filterName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(_filterFeeType) && Enum.TryParse<FeeType>(_filterFeeType, out var ft))
            q = q.Where(t => t.FeeType == ft);

        if (!string.IsNullOrWhiteSpace(_filterContractorType) && Enum.TryParse<ContractorType>(_filterContractorType, out var ct))
            q = q.Where(t => t.ContractorTypes.Contains(ct));

        if (_filterComment == "true")
            q = q.Where(t => t.AllowComment);
        else if (_filterComment == "false")
            q = q.Where(t => !t.AllowComment);

        if (_filterFieldsMin.HasValue)
            q = q.Where(t => t.Fields.Count >= _filterFieldsMin.Value);
        if (_filterFieldsMax.HasValue)
            q = q.Where(t => t.Fields.Count <= _filterFieldsMax.Value);

        if (_filterActive == "true")
            q = q.Where(t => t.IsActive);
        else if (_filterActive == "false")
            q = q.Where(t => !t.IsActive);

        _filtered = q.OrderBy(t => t.FeeType).ThenBy(t => t.Name).ToList();
    }

    private void ShowAddForm()
    {
        _editingTemplate = null;
        _formTemplate = new DeclarationTemplate();
        _showForm = true;
        _message = null;
    }

    private void EditTemplate(DeclarationTemplate template)
    {
        _editingTemplate = template;
        _formTemplate = new DeclarationTemplate
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            FeeType = template.FeeType,
            ContractorTypes = new List<ContractorType>(template.ContractorTypes),
            Fields = template.Fields.Select(f => new TemplateField
            {
                Number = f.Number,
                Code = f.Code,
                Name = f.Name,
                DataType = f.DataType,
                IsRequired = f.IsRequired,
                Unit = f.Unit
            }).ToList(),
            AllowComment = template.AllowComment,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
        _showForm = true;
        _message = null;
    }

    private async Task SaveTemplate()
    {
        _message = null;
        _isError = false;

        if (string.IsNullOrWhiteSpace(_formTemplate.Name))
        {
            _message = "Nazwa jest wymagana.";
            _isError = true;
            return;
        }
        if (_formTemplate.ContractorTypes.Count == 0)
        {
            _message = "Wybierz co najmniej jeden typ kontrahenta.";
            _isError = true;
            return;
        }

        if (_editingTemplate is not null)
        {
            await TemplateService.UpdateAsync(_formTemplate);
            _message = "Wzorzec zaktualizowany.";
        }
        else
        {
            await TemplateService.CreateAsync(_formTemplate);
            _message = "Wzorzec utworzony.";
        }

        _showForm = false;
        await LoadTemplates();
    }

    private void CancelForm()
    {
        _showForm = false;
        _message = null;
    }

    private async Task DeleteTemplate(string id)
    {
        await TemplateService.DeleteAsync(id);
        _message = "Wzorzec usuniety.";
        _isError = false;
        await LoadTemplates();
    }

    private void AddField()
    {
        _formTemplate.Fields.Add(new TemplateField());
    }

    private void RemoveField(int index)
    {
        _formTemplate.Fields.RemoveAt(index);
    }

    private void ToggleContractorType(ContractorType type, bool isChecked)
    {
        if (isChecked && !_formTemplate.ContractorTypes.Contains(type))
            _formTemplate.ContractorTypes.Add(type);
        else if (!isChecked)
            _formTemplate.ContractorTypes.Remove(type);
    }
}
