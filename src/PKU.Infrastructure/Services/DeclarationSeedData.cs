using PKU.Domain.Entities;
using PKU.Domain.Enums;

namespace PKU.Infrastructure.Services;

public static class DeclarationSeedData
{
    public static List<Declaration> Create()
    {
        var now = DateTime.Now;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        var prevMonth = currentMonth == 1 ? 12 : currentMonth - 1;
        var prevYear = currentMonth == 1 ? currentYear - 1 : currentYear;

        return
        [
            // === OSDp (user "2") - previous month: 2 submitted ===
            new Declaration
            {
                Id = "seed-osdp-op-prev",
                UserId = "2",
                ContractorType = ContractorType.OSDp,
                FeeType = FeeType.OP,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = prevYear,
                BillingMonth = prevMonth,
                Status = DeclarationStatus.Submitted,
                DeclarationNumber = $"OSW/OP/OSDp/{prevYear}/{prevMonth:D2}/01/01",
                SubmittedAt = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(4),
                Deadline = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(9)
            },
            new Declaration
            {
                Id = "seed-osdp-oze-prev",
                UserId = "2",
                ContractorType = ContractorType.OSDp,
                FeeType = FeeType.OZE,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = prevYear,
                BillingMonth = prevMonth,
                Status = DeclarationStatus.Submitted,
                DeclarationNumber = $"OSW/OZE/OSDp/{prevYear}/{prevMonth:D2}/01/01",
                SubmittedAt = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(5),
                Deadline = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(9)
            },
            // OSDp - current month: 1 draft (OKO), 1 submitted (OP)
            new Declaration
            {
                Id = "seed-osdp-oko-curr",
                UserId = "2",
                ContractorType = ContractorType.OSDp,
                FeeType = FeeType.OKO,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = currentYear,
                BillingMonth = currentMonth,
                Status = DeclarationStatus.Draft,
                Deadline = new DateTime(currentYear, currentMonth, 1).AddMonths(1).AddDays(9)
            },
            new Declaration
            {
                Id = "seed-osdp-op-curr",
                UserId = "2",
                ContractorType = ContractorType.OSDp,
                FeeType = FeeType.OP,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = currentYear,
                BillingMonth = currentMonth,
                Status = DeclarationStatus.Submitted,
                DeclarationNumber = $"OSW/OP/OSDp/{currentYear}/{currentMonth:D2}/01/01",
                SubmittedAt = DateTime.Now.AddDays(-2),
                Deadline = new DateTime(currentYear, currentMonth, 1).AddMonths(1).AddDays(9)
            },

            // === OSDn (user "3") - previous month: 1 submitted ===
            new Declaration
            {
                Id = "seed-osdn-oze-prev",
                UserId = "3",
                ContractorType = ContractorType.OSDn,
                FeeType = FeeType.OZE,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = prevYear,
                BillingMonth = prevMonth,
                Status = DeclarationStatus.Submitted,
                DeclarationNumber = $"OSW/OZE/OSDn/{prevYear}/{prevMonth:D2}/01/01",
                SubmittedAt = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(3),
                Deadline = new DateTime(prevYear, prevMonth, 1).AddMonths(1).AddDays(9)
            },

            // === Wytworca (user "5") - current month: 1 submitted ===
            new Declaration
            {
                Id = "seed-wyt-om-curr",
                UserId = "5",
                ContractorType = ContractorType.Wyt,
                FeeType = FeeType.OM,
                FeeCategory = FeeCategory.Pozaprzesylowa,
                BillingYear = currentYear,
                BillingMonth = currentMonth,
                Status = DeclarationStatus.Submitted,
                DeclarationNumber = $"OSW/OM/Wyt/{currentYear}/{currentMonth:D2}/01/01",
                SubmittedAt = DateTime.Now.AddDays(-1),
                Deadline = new DateTime(currentYear, currentMonth, 1).AddMonths(1).AddDays(9)
            },
        ];
    }
}
