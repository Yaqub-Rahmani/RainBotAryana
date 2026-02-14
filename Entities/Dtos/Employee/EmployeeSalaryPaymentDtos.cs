using Entities.Concrete;
using System;

namespace Entities.Dtos.Employee
{
    public class EmployeeSalaryPaymentListItemDto
    {
        public Guid ExpenseId { get; set; }
        public Guid MoneyEntryId { get; set; }

        public DateTime OccurredAtUtc { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        public Guid MoneyAccountId { get; set; }
        public string MoneyAccountName { get; set; } = "";

        public PaymentMethod PaymentMethod { get; set; }
        public string DisplayName { get; set; } = "";
        public string? Notes { get; set; }

        public decimal AfnPerUsdAtThatTime { get; set; }
        public decimal UsdEquivalent { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
