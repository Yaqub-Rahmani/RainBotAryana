using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public sealed class ContractListItemDto : IDto
    {
        public Guid ContractId { get; init; }
        public Guid ShopId { get; init; }
        public Guid TenantId { get; init; }
        public Guid MarketId { get; init; }
        public Guid FloorId { get; init; }
        public string MarketName { get; init; } = "";
        public string FloorName { get; init; } = "";
        public string ShopName { get; init; } = "";
        public string TenantName { get; init; } = "";
        public string BusinessActivity { get; set; } = "";

        public string FranchiseSummary { get; init; } = "";
        public int FranchiseOwnersCount { get; init; }
        public string? TopFranchiseOwnerName { get; init; }

        public string FranchiseName { get; init; } = "";

        public DateOnly ContractStartDate { get; init; }
        public DateOnly ContractEndDate { get; init; }

        public decimal OwnerPortionAmount { get; init; }
        public decimal MarketPortionAmount { get; init; }
        public decimal FranchisePortionAmount { get; init; }
        public decimal TotalFixedRent { get; init; }

        public decimal? ElectricityThisPeriodTotal { get; init; }

        public decimal OldDebtTotal { get; set; }

        /// <summary>Bu kabul döneminde (curY/curM) üretilen toplam borç (Amount toplamı)</summary>
        public decimal CurrentPeriodAccrualTotal { get; set; }

        /// <summary>Bu kabul döneminde henüz ödenmemiş borç (Outstanding toplamı)</summary>
        public decimal CurrentPeriodOutstanding { get; set; }

        /// <summary>Geçmiş kabul dönemlerinden devreden, hâlâ kapatılmamış borç</summary>
        public decimal OldOutstanding { get; set; }

        /// <summary>Toplam ödenmemiş borç = Eski + Bu dönem</summary>
        public decimal TotalOutstanding { get; set; }
        public int DueDayOfMonth { get; init; }
        public ContractStatus Status { get; init; }
    }

}
