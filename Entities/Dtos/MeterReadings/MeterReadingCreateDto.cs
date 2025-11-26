using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MeterReadings
{
    public class MeterReadingCreateDto:IDto
    {
        public Guid MeterReadingId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public decimal PreviousIndex { get; set; }
        public decimal CurrentIndex { get; set; }
        public decimal UnitPrice { get; set; } // >= 0
        public decimal ReserveRate { get; set; } // 0..1
        public string? Notes { get; set; }

        public bool IsConfirmed { get; set; }
    }
}
