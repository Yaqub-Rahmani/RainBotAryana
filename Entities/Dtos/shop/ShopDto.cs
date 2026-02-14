using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.shop
{
    public class ShopDto: IDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public Guid MarketId { get; set; }
        public string MarketName { get; set; } = null!;

        public Guid FloorId { get; set; }
        public string FloorName { get; set; } = null!;

        // Sözleşme & kiracı
        public bool HasActiveContract { get; set; }
        public bool HasTenant { get; set; }   // Kural: sözleşme varsa true

        public string? TenantName { get; set; }
        public string? BusinessActivity { get; set; }

        // İmtiyaz
        public bool HasFranchiseOwner { get; set; }
        public string? FranchiseOwnerSummary { get; set; }
        public decimal? AreaM2 { get; set; }
        public DateOnly? ContractEndDate { get; set; }
        public int? ContractRemainingDays { get; set; }
        public bool IsActive { get; set; }
    }

    // Detay ekranı için (ShopDto'dan miras alıp genişletebilirsin)
    public class ShopDetailDtooo : ShopDto
    {
        public string? Description { get; set; }
        public decimal? AreaM2 { get; set; }
        public bool IsActive { get; set; }
    }

    public class ShopCreateDto
    {
        public Guid MarketId { get; set; }
        public Guid FloorId { get; set; }

        public string? Name { get; set; }

        public decimal? AreaM2 { get; set; }
    }

    public class ShopUpdateDto : ShopCreateDto
    {
        public Guid Id { get; set; }
    }
}
