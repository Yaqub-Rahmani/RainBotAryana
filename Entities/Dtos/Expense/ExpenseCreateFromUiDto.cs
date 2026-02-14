using Core.Entities;
using Entities.Concrete;
using System;

namespace Entities.Dtos.Expense
{
    /// <summary>
    /// UI -> API create DTO for Expense screen.
    /// Notes:
    /// - Amount should always be positive.
    /// - Direction controls whether the movement is money-in (refund) or money-out.
    /// </summary>
    public sealed class ExpenseCreateFromUiDto : IDto
    {
        public string DisplayName { get; set; } = null!;

        public decimal Amount { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Unknown;
        public ExpenseSource Source { get; set; } = ExpenseSource.Unknown;

        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// If omitted, server will use current UTC.
        /// </summary>
        public DateTime? OccurredAtUtc { get; set; }

        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        public Guid MoneyAccountId { get; set; }
        public Currency Currency { get; set; } = Currency.Afn;
        public BalanceDirection Direction { get; set; } = BalanceDirection.Out;
    }
}
