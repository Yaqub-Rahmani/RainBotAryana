using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ShopDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? UnitNumber { get; set; }
        public bool IsActive { get; set; }

        // Market & Floor
        public Guid MarketId { get; set; }
        public string MarketName { get; set; } = "";
        public Guid FloorId { get; set; }
        public string FloorName { get; set; } = "";

        // Counters
        public int FranchiseShareCount { get; set; }
        public int ContractCount { get; set; }
    }
}
