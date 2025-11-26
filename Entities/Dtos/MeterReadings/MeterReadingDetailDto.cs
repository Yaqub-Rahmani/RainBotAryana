using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MeterReadings
{
    public class MeterReadingDetailDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateOnly ReadingDate { get; set; }


        public decimal PreviousIndex { get; set; }
        public decimal CurrentIndex { get; set; }
        public decimal ConsumptionKwh { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ReserveRate { get; set; }
        public decimal ReserveAmount { get; set; }
        public decimal TotalBilled { get; set; }


        public bool IsFinalized { get; set; }
        public int? FinalizedByUserId { get; set; }
        public DateTimeOffset? FinalizedAt { get; set; }


        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }


        public string? Notes { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
