using Core.Entities;
using Entities.Concrete;
using Entities.Dtos.contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractDetailDto
    {
        // Kimlikler
        public Guid ContractId { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public Guid MarketId { get; set; }
        public Guid FloorId { get; set; }

        // İsimler
        public string MarketName { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;

        // Tarihler
        public DateOnly SignedOn { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DueDayOfMonth { get; set; }

        // Kira – sabit tutarlar
        public decimal OwnerPortionAmount { get; set; }
        public decimal MarketPortionAmount { get; set; }
        public decimal FranchisePortionAmount { get; set; }
        public decimal TotalFixedRent { get; set; }

        // İş faaliyeti ve durum
        public string BusinessActivity { get; set; } = string.Empty;
        public ContractStatus Status { get; set; }

        // Sözleşme meta
        public string? ContractVersion { get; set; }
        public string? ContractNote { get; set; }
        public string? ContractArchiveReason { get; set; }

        // Elektrik
        public string? ElectricityNote { get; set; }
        public decimal? InitialElectricityIndex { get; set; }

        // Franchise özet
        public int FranchiseOwnersCount { get; set; }
        public string? TopFranchiseOwnerName { get; set; }
        public string FranchiseSummary { get; set; } = string.Empty;
        public List<FranchiseOwnerShareDto> FranchiseOwners { get; set; } = new();

        // Borç / tahakkuk
        public decimal CurrentPeriodAccrualTotal { get; set; }
        public decimal CurrentPeriodOutstanding { get; set; }
        public decimal OldOutstanding { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OldDebtTotal { get; set; }

        // Detay listeleri
        public List<AccrualHistoryItemDto> AccrualHistory { get; set; } = new();
        public List<CollectionHistoryItemDto> CollectionHistory { get; set; } = new();
    }
}

