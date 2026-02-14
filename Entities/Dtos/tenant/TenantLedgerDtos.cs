using Core.Utilities.Paging;
using Core.Utilities.Query;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.tenant
{
    public class TenantAccrualRowDto
    {
        public Guid AccrualId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public AllocationKind Kind { get; set; }
        public AccrualStatus Status { get; set; }

        public decimal Amount { get; set; }
        public decimal OutstandingAmount { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

    public class TenantAccrualQueryOptions : QueryOptions { }

    public class TenantAccrualPageDto
    {
        public PagedResult<TenantAccrualRowDto> Page { get; set; } = default!;
        public decimal TotalAccrued { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public class TenantAccrualSettlementLineDto
    {
        public Guid SettlementId { get; set; }
        public decimal SettledAmount { get; set; }
        public DateTime SettledAtUtc { get; set; }

        public Guid CollectionId { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public decimal CollectionAmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReferenceNo { get; set; }
        public bool IsApproved { get; set; }

        public AllocationKind AllocationKind { get; set; }
        public Guid AllocationId { get; set; }
    }

    public class TenantAccrualDetailDto
    {
        public Guid AccrualId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public AllocationKind Kind { get; set; }
        public AccrualStatus Status { get; set; }

        public decimal Amount { get; set; }
        public decimal OutstandingAmount { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public List<TenantAccrualSettlementLineDto> Settlements { get; set; } = new();
    }

    // -------------------- COLLECTIONS (ÖDEMELER) --------------------
    public class TenantCollectionRowDto
    {
        public Guid CollectionId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public DateTime PaidAtUtc { get; set; }
        public decimal AmountReceived { get; set; }

        public PaymentMethod Method { get; set; }
        public bool IsApproved { get; set; }
        public string? ReferenceNo { get; set; }

        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
    }

    public class TenantCollectionQueryOptions : QueryOptions { }

    public class TenantCollectionPageDto
    {
        public PagedResult<TenantCollectionRowDto> Page { get; set; } = default!;
        public decimal TotalReceived { get; set; }
        public int ApprovedCount { get; set; }
        public int NotApprovedCount { get; set; }
    }

    public class TenantCollectionAllocationLineDto
    {
        public Guid AllocationId { get; set; }
        public AllocationKind Kind { get; set; }
        public decimal Amount { get; set; }
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public string? Notes { get; set; }
    }

    public class TenantCollectionSettlementLineDto
    {
        public Guid SettlementId { get; set; }
        public decimal SettledAmount { get; set; }
        public DateTime SettledAtUtc { get; set; }

        public Guid AllocationId { get; set; }
        public AllocationKind AllocationKind { get; set; }

        public Guid AccrualId { get; set; }
        public int AccrualYear { get; set; }
        public int AccrualMonth { get; set; }
        public AllocationKind AccrualKind { get; set; }
        public decimal AccrualAmount { get; set; }
        public decimal AccrualOutstandingAmount { get; set; }
    }

    public class TenantCollectionDetailDto
    {
        public Guid CollectionId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public DateTime PaidAtUtc { get; set; }
        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public bool IsApproved { get; set; }
        public string? ReferenceNo { get; set; }
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
        public string? Notes { get; set; }

        public List<TenantCollectionAllocationLineDto> Allocations { get; set; } = new();
        public List<TenantCollectionSettlementLineDto> Settlements { get; set; } = new();
    }
}
