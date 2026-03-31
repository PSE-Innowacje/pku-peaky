using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;
using PKU.Domain.Services;

namespace PKU.Infrastructure.Services;

public class InMemoryDeclarationService : IDeclarationService
{
    private readonly List<Declaration> _declarations;
    private readonly IUserService _userService;

    public InMemoryDeclarationService(IUserService userService)
    {
        _userService = userService;
        _declarations = DeclarationSeedData.Create();
    }

    public Task<IEnumerable<Declaration>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Declaration>>(_declarations);

    public Task<Declaration?> GetByIdAsync(string id) =>
        Task.FromResult(_declarations.FirstOrDefault(d => d.Id == id));

    public Task<IEnumerable<Declaration>> GetPendingAsync() =>
        Task.FromResult<IEnumerable<Declaration>>(
            _declarations.Where(d => d.Status == DeclarationStatus.Submitted).ToList());

    public Task CreateAsync(Declaration declaration)
    {
        _declarations.Add(declaration);
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(string id, DeclarationStatus status)
    {
        var declaration = _declarations.FirstOrDefault(d => d.Id == id);
        if (declaration is not null) declaration.Status = status;
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Declaration>> GetForContractorMonthAsync(string userId, int year, int month)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user is null || user.Role != UserRole.Kontrahent)
            return [];

        var feeTypes = ContractorFeeMapping.GetFeeTypesForContractor(user.ContractorType);

        var existing = _declarations
            .Where(d => d.UserId == userId && d.BillingYear == year && d.BillingMonth == month)
            .ToList();

        var result = new List<Declaration>();

        foreach (var feeType in feeTypes)
        {
            var declaration = existing.FirstOrDefault(d => d.FeeType == feeType);
            if (declaration is null)
            {
                var feeCategory = DeclarationNumberGenerator.GetFeeCategory(feeType);
                declaration = new Declaration
                {
                    UserId = userId,
                    ContractorType = user.ContractorType,
                    FeeType = feeType,
                    FeeCategory = feeCategory,
                    BillingYear = year,
                    BillingMonth = month,
                    Status = DeclarationStatus.NotSubmitted,
                    Deadline = new DateTime(year, month, 1).AddMonths(1).AddDays(9)
                };
                _declarations.Add(declaration);
            }
            result.Add(declaration);
        }

        return result;
    }
}
