using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public sealed class ShopShareSummaryDto : IDto
    {
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public Guid MarketId { get; set; }
        public Guid FloorId { get; set; }
        public string? UnitNumber { get; set; }
        public bool IsActive { get; set; }

        public decimal? PercentFilled { get; set; } // 0..100
        public decimal? PercentOpen { get; set; } // 0..100
        public decimal? PercentMy { get; set; } // 0..100
        public int OwnersCount { get; set; } // opsiyonel ama faydalı
    }

}
