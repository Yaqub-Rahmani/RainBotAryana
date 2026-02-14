using Core.Entities;
using Entities.Concrete;
using System;

namespace Entities.Dtos.Expense
{
    public sealed class ExpenseUpdateFromUiDto : IDto
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = null!;

        public decimal Amount { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Unknown;
        public ExpenseSource Source { get; set; } = ExpenseSource.Unknown;

        public Guid? EmployeeId { get; set; }

        public DateTime? OccurredAtUtc { get; set; }

        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        public Guid MoneyAccountId { get; set; }
        public Currency Currency { get; set; } = Currency.Afn;
        public BalanceDirection Direction { get; set; } = BalanceDirection.Out;
    }
}
