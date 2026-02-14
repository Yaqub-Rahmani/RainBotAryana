using Core.Entities;
using System;

namespace Entities.Dtos.Dashboard
{
    public enum DashboardEntrySource
    {
        MoneyAccount = 1,
        ReserveAccount = 2,
        FamilyMember = 3,
        FranchiseOwner = 4,
        TaxPool = 5,
        ZakatPool = 6,
        CharityPool = 7,

        Collection = 8,     // Ödenen para -> hangi borçları kapattı
        AccrualDebt = 9     // Borç tablosu -> borç nedeni ve detay satırları
    }



    public class DashboardEntryDto : IDto
    {
        public string Source { get; set; } = "";
        public Guid? SourceId { get; set; }

        public Guid Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string Direction { get; set; } = "";
        public decimal Amount { get; set; }

        public string? Title { get; set; }
        public string? Notes { get; set; }
        public string? Method { get; set; }
        public string? Reference { get; set; }

        public string? Currency { get; set; }
        public decimal? AfnPerUsdAtThatTime { get; set; }
        public decimal? UsdEquivalent { get; set; }

        // ✅ Yeni (eksik alanlar)
        public Guid? TenantId { get; set; }
        public string? TenantName { get; set; }

        public string? MarketName { get; set; }
        public string? FloorName { get; set; }
        public string? ShopUnitNumber { get; set; }

        public string? Kind { get; set; }          // Electricity/Market/Owner/Franchise
        public int? PeriodYear { get; set; }       // 1402...
        public int? PeriodMonth { get; set; }      // 1..12

        // Borç/settlement için
        public Guid? AccrualId { get; set; }
        public decimal? AccrualAmount { get; set; }
        public decimal? OutstandingAmount { get; set; }

        public Guid? CollectionId { get; set; }    // entry hangi ödeme ile bağlı
    }


    public class DashboardEntryQueryDto : IDto
    {
        public DashboardEntrySource Source { get; set; }
        public Guid? SourceId { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public int Take { get; set; } = 100;
    }
}
