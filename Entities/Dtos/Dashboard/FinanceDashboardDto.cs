using Core.Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;

namespace Entities.Dtos.Dashboard
{
    public class FinanceDashboardDto : IDto
    {
        public DateTime GeneratedAtUtc { get; set; }
        // DB'de kayıtlı en son USD kuru (DollarRate tablosu)
        public decimal? LatestAfnPerUsd { get; set; }
        public DateTime? LatestAfnPerUsdEffectiveFromUtc { get; set; }
        public DashboardMoneyTotalsDto Totals { get; set; } = new();
        public CashBankTotalsDto CashBank { get; set; } = new();
        public AccountBucketsDto Buckets { get; set; } = new();
        public DashboardBreakdownsDto Breakdowns { get; set; } = new();
        public DashboardTabsDto Tabs { get; set; } = new();
    }

    public class DashboardMoneyTotalsDto : IDto
    {
        public decimal TotalUsd { get; set; }
        public decimal TotalAfn { get; set; }
        public decimal? TotalUsdInAfn { get; set; }
        public decimal? TotalAfnInUsd { get; set; }
    }

    public class CashBankTotalsDto : IDto
    {
        public decimal CashUsd { get; set; }
        public decimal CashAfn { get; set; }
        public decimal BankUsd { get; set; }
        public decimal BankAfn { get; set; }
        public List<MoneyAccountBalanceItemDto> Accounts { get; set; } = new();
    }

    public class MoneyAccountBalanceItemDto : IDto
    {
        public Guid MoneyAccountId { get; set; }
        public string AccountName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }

    public class AccountBucketsDto : IDto
    {
        public decimal FamilyPoolAfn { get; set; }
        public decimal FranchisePoolAfn { get; set; }
        public decimal MarketMainAfn { get; set; }
        public decimal ElectricityMainAfn { get; set; }
        public decimal ReserveTotalAfn { get; set; }
        public decimal TaxPoolAfn { get; set; }
        public decimal ZakatPoolAfn { get; set; }
        public decimal CharityPoolAfn { get; set; }
        public decimal? FamilyPoolUsd { get; set; }
        public decimal? FranchisePoolUsd { get; set; }
        public decimal? MarketMainUsd { get; set; }
        public decimal? ElectricityMainUsd { get; set; }
        public decimal? ReserveTotalUsd { get; set; }
        public decimal? TaxPoolUsd { get; set; }
        public decimal? ZakatPoolUsd { get; set; }
        public decimal? CharityPoolUsd { get; set; }
    }

    public class DashboardBreakdownsDto : IDto
    {
        public List<FamilyBalanceSummaryDto> FamilyMembers { get; set; } = new();
        public List<FranchiseBalanceSummaryDto> FranchiseOwners { get; set; } = new();
        public List<ReserveAccountBalanceDto> ReserveAccounts { get; set; } = new();
    }

    public class DashboardTabsDto : IDto
    {
        public List<CollectionDashboardItemDto> ApprovedCollections { get; set; } = new();
        public List<CollectionDashboardItemDto> PendingCollections { get; set; } = new();
        public List<PeriodDebtDashboardItemDto> PeriodDebts { get; set; } = new();
    }

    public class CollectionDashboardItemDto : IDto
    {
        public Guid Id { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public string TenantName { get; set; } = "";
        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public decimal AmountReceived { get; set; }
        public string Method { get; set; } = "";
        public string? ReferenceNo { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
    }

    public class PeriodDebtDashboardItemDto : IDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAccrued { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}
