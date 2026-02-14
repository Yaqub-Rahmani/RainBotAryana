using Core.Entities;
using Entities.Concrete;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos.employee
{
    public class EmployeeTransactionListItemDto : IDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }

        public BalanceDirection Direction { get; set; }
        public decimal Amount { get; set; }
        public Guid? MoneyEntryId { get; set; }

        [Range(1300, 1500)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public DateTime PaidAtUtc { get; set; }
    }

    public class EmployeeTransactionCreateDto : IDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; }

        [Range(0.01, 999999999)]
        public decimal Amount { get; set; }

        // Optional. If null/0 => set from KabulTimeProvider (Persian year/month)
        public int? Year { get; set; }

        // Optional. If null/0 => set from KabulTimeProvider (Persian year/month)
        public int? Month { get; set; }

        // Optional. If null => now (UTC)
        public DateTime? PaidAtUtc { get; set; }

        // Optional. Link to MoneyEntry created by general payment model
        public Guid? MoneyEntryId { get; set; }
    }
}
