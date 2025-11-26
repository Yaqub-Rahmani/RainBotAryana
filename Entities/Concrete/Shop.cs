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
    //    1) Çekirdek Varlıklar
    //Shop(Dükkân)
    //Bir dükkânı temsil eder.
    //Fiziksel konum bilgilerini (kat, numara) saklar.
    //Bir dükkânda tek aktif kiracı olacak kuralı ileride Sözleşme katmanında uygulanacak; burada sadece referans varlık.

    public class Shop : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        // --- Yeni FK'ler ---
        /// <summary>Bağlı olduğu Market</summary>
        [Required]
        public Guid MarketId { get; set; }

        /// <summary>Bağlı olduğu Kat (MarketFloor)</summary>
        [Required]
        public Guid FloorId { get; set; }

        /// <summary>Dükkan adı veya görünen başlık (opsiyonel)</summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>Dükkan/ünite numarası (ör. 105, A12)</summary>
        [MaxLength(50)]
        public string? UnitNumber { get; set; }

        public bool IsActive { get; set; } = true;

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
