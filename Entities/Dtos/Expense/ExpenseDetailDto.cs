using Core.Entities;
using Entities.Concrete;
using System;

namespace Entities.Dtos.Expense
{
    public sealed class ExpenseDetailDto : IDto
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = "";

        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public string CurrencyLabel { get; set; } = ""; // "؋" / "USD"

        public BalanceDirection Direction { get; set; }
        public string DirectionLabel { get; set; } = "";

        public ExpenseCategory Category { get; set; }
        public string CategoryLabel { get; set; } = "";

        public ExpenseSource Source { get; set; }
        public string SourceLabel { get; set; } = "";

        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public DateTime OccurredAtUtc { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodLabel { get; set; } = "";

        public string? Notes { get; set; }

        public Guid? MoneyEntryId { get; set; }
        public Guid MoneyAccountId { get; set; }
        public string MoneyAccountName { get; set; } = "";

        public decimal UsdEquivalent { get; set; }
        public decimal AfnPerUsdAtThatTime { get; set; }

        public bool IsActive { get; set; }
    }
}
