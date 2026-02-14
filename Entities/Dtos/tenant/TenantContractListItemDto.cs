using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.tenant
{
    public class TenantContractListItemDto : IDto
    {
        public Guid ContractId { get; set; }
        public string ContractStatus { get; set; } = "";

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? UnitNumber { get; set; }

        public Guid MarketId { get; set; }
        public string? MarketName { get; set; }

        public Guid FloorId { get; set; }
        public string? FloorName { get; set; }
    }
}
