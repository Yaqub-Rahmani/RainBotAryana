using Core.Entities;
using System;

namespace Entities.Dtos.Dashboard
{
    public sealed class MoneyAccountBalanceTableRowDto : IDto
    {
        public Guid Id { get; set; }                 // row id (MoneyAccountId)
        public string AccountName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class ReserveAccountBalanceTableRowDto : IDto
    {
        public Guid Id { get; set; }                 // ReserveAccountId
        public string AccountType { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Pool breakdown (Family / Franchise): per person balance + debt
    /// DebtAfn: if Out > In => Out-In else 0
    /// </summary>
    public sealed class PoolPartyBalanceRowDto : IDto
    {
        public Guid Id { get; set; }                 // PartyId
        public Guid PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public decimal TotalInAfn { get; set; }
        public decimal TotalOutAfn { get; set; }
        public decimal BalanceAfn { get; set; }
        public decimal DebtAfn { get; set; }
    }

    public sealed class CollectionDashboardRowDto : IDto
    {
        public Guid Id { get; set; }

        public DateTime PaidAtUtc { get; set; }

        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = "";

        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? ShopUnitNumber { get; set; }
        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";

        public decimal AmountReceived { get; set; }
        public string Method { get; set; } = "";
        public string? ReferenceNo { get; set; }

        public bool IsApproved { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }

        public string? Notes { get; set; }
    }

    public sealed class PeriodDebtRowDto : IDto
    {
        public string Id { get; set; } = "";         // composite key
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = "";
        public int Year { get; set; }                // (1300..1500)
        public int Month { get; set; }               // 1..12
        public decimal TotalAccrued { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public sealed class DashboardChartsDto : IDto
    {
        public int Months { get; set; }
        public ChartMonthItemDto[] Items { get; set; } = Array.Empty<ChartMonthItemDto>();
    }

    public sealed class ChartMonthItemDto : IDto
    {
        public string Key { get; set; } = "";        // "YYYY-MM"
        public string Label { get; set; } = "";      // same or localized
        public decimal CollectionsAfn { get; set; }
        public decimal AccruedAfn { get; set; }
        public decimal OutstandingAfn { get; set; }
    }
}
