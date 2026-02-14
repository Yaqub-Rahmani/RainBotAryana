using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Shop : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MarketId { get; set; }

        [Required]
        public Guid FloorId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? UnitNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal? AreaM2 { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(MarketId))]
        public Market Market { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FloorId))]
        public Floor Floor { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ShopFranchiseShare> ShopFranchiseShares { get; set; } = new List<ShopFranchiseShare>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Accrual> Accruals { get; set; } = new List<Accrual>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CollectionAllocation> Allocations { get; set; } = new List<CollectionAllocation>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FranchiseBalanceEntry> FranchiseBalanceEntries { get; set; } = new List<FranchiseBalanceEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();        
        
        [JsonIgnore]
        [ValidateNever]
        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }
}
