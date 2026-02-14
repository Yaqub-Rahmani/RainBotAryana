using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Accrual
{
    public class AccrualCreateDto: IDto
    {
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ContractId { get; set; }
        public Guid? MeterReadingId { get; set; }

        public AllocationKind Kind { get; set; }
        public int Year { get; set; }      // 1300–1500 (هجری شمسی)
        public int Month { get; set; }     // 1–12

        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
