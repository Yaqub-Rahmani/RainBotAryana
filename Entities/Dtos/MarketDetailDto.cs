using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class MarketDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string MarketName { get; set; } = "";
        public bool Status { get; set; }

        // İlişki özetleri
        public int FloorCount { get; set; }
        public int ActiveFloorCount { get; set; }
        public int ShopCount { get; set; }
    }
}
