using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Accrual
{
    public class AccrualDetailDto: IDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ContractId { get; set; }
        public Guid? MeterReadingId { get; set; }
        public AllocationKind Kind { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public AccrualStatus Status { get; set; }
        public decimal Amount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string? Notes { get; set; }

        // Zengin alan örnekleri
        public int SettlementCount { get; set; }
        public decimal SettledTotal { get; set; }
    }
}
