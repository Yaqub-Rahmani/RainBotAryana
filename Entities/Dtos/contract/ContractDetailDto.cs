using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractDetailDto :IDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public DateOnly SignedOn { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DueDayOfMonth { get; set; }
        public decimal OwnerPortionAmount { get; set; }
        public decimal FranchisePortionAmount { get; set; }
        public decimal MarketPortionAmount { get; set; }
        public decimal TotalFixedRent { get; set; }
        public string? ElectricityNote { get; set; }
        public string? BusinessActivity { get; set; }
        public ContractStatus Status { get; set; }
        public string? Version { get; set; }
        public string? ArchiveReason { get; set; }

        // Örnek zengin alanlar
        public int AccrualCount { get; set; }
        public int CollectionCount { get; set; }
    }
}

