using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class MeterReadingDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = "";

        public int Year { get; set; }
        public int Month { get; set; }
        public DateOnly ReadingDate { get; set; }

        public decimal PreviousIndex { get; set; }
        public decimal CurrentIndex { get; set; }
        public decimal ConsumptionKwh { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal ReserveRate { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ReserveAmount { get; set; }
        public decimal TotalBilled { get; set; }

        public bool IsFinalized { get; set; }
        public string? Notes { get; set; }

        // Accrual özeti (varsa)
        public Guid? AccrualId { get; set; }
        public decimal? AccrualAmount { get; set; }
        public decimal? AccrualOutstanding { get; set; }
        public string? AccrualStatus { get; set; } // Open/PartiallySettled/Settled
    }
}
