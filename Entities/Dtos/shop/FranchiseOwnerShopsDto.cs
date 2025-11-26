using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.shop
{
    public class FranchiseOwnerShopsDto : IDto
    {
        public Guid OwnerId { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ContractId { get; set; }
        public Guid MarketId { get; set; }
        public Guid FloorId { get; set; }
        public string TenantName { get; set; }
        public string ShopName { get; set; }
        public string FloorName { get; set; }
        public string MarketName { get; set; }

        // ✅ Tarihler artık string
        public string ContractStartDate { get; set; }
        public string ContractEndDate { get; set; }
        public string ContractSignedOn { get; set; }

        public int DueDayOfMonth { get; set; }
        public decimal TotalFranchisePortionAmount { get; set; }
        public decimal MyPortionAmount { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Archived;
        public decimal SharePercent { get; set; }
    }

}
