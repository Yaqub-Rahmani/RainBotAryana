using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Collection
{
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public DateTime PaidAtUtc { get; set; }
        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReferenceNo { get; set; }

        public Guid? MoneyEntryId { get; set; }

        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }

        public bool IsApproved { get; set; }
        public int CreatedByUserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        public string? Notes { get; set; }
    }

    public class CollectionCreateDto
    {
        public Guid TenantId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public DateTime? PaidAtUtc { get; set; }      // null gelirse _timeProvider kullanılacak

        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReferenceNo { get; set; }

        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Para hangi hesaba girdi? (Kasa AFN, Bank AFN, Bank USD vb.)
        /// </summary>
        public Guid MoneyAccountId { get; set; }
    }

    public class CollectionApproveDto
    {
        public Guid Id { get; set; }
    }

    ////////////// collection outo add
    ///

    public class ContractDebtPaymentRequestDto
    {
        public Guid ContractId { get; set; }

        public ContractDebtPaymentInfoDto Payment { get; set; } = null!;

        public List<ContractDebtAllocationDto> Allocations { get; set; } = new();
    }

    public class ContractDebtPaymentInfoDto
    {
        public string Method { get; set; } = null!;     // "bank", "cash" vs. (map edeceğiz)
        public string? MethodLabel { get; set; }        // UI için, kullanmak şart değil
        public string? Reference { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        public DateTime? PaidAtUtc { get; set; }
        public Guid MoneyAccountId { get; set; }        // Bunu frontend’ten mutlaka alman lazım
    }

    public class ContractDebtAllocationDto
    {
        public Guid DebtId { get; set; }    // Accrual.Id
        public decimal Amount { get; set; } // Bu ödeme turunda bu borca giden para
    }

}
