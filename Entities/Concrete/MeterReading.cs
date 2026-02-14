using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete
{
    public class MeterReading : IEntity
    {
        [Key] public Guid Id { get; set; }

        [Required] public Guid ShopId { get; set; }
        [Required] public Guid ContractId { get; set; }

        [Range(1300, 1500)] public int Year { get; set; }
        [Range(1, 12)] public int Month { get; set; }

        [Required] public DateOnly ReadingDate { get; set; }

        // Audit
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }             
        public DateTimeOffset? UpdatedAt { get; set; }            


        [Precision(18, 3)] public decimal PreviousIndex { get; set; }
        [Precision(18, 3)] public decimal CurrentIndex { get; set; }

        [NotMapped] public decimal ConsumptionKwh => Math.Max(0, CurrentIndex - PreviousIndex);

        // Fiyat & tutarlar
        [Precision(18, 4)] public decimal UnitPrice { get; set; }

        [NotMapped] public decimal NetAmount => Math.Round(ConsumptionKwh * UnitPrice, 2, MidpointRounding.AwayFromZero);

        [Precision(5, 4), Range(0, 1)]
        public decimal ReserveRate { get; set; } = 0.10m;

        [NotMapped] public decimal ReserveAmount => Math.Round(NetAmount * ReserveRate, 2, MidpointRounding.AwayFromZero);
        [NotMapped] public decimal TotalBilled => NetAmount + ReserveAmount;

        [MaxLength(500)] public string? Notes { get; set; }

        // İş akışı
        public bool IsFinalized { get; set; }
        public int? FinalizedByUserId { get; set; }           
        public DateTimeOffset? FinalizedAt { get; set; }    

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>(); 

        // Navigation
        [JsonIgnore, ValidateNever, ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore, ValidateNever, ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        [JsonIgnore, ValidateNever] public ICollection<Accrual> Accruals { get; set; } = new List<Accrual>();
        [JsonIgnore, ValidateNever] public ICollection<CollectionAllocation> Allocations { get; set; } = new List<CollectionAllocation>();
        [JsonIgnore, ValidateNever] public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();

    }
}
