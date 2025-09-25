using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public Market? Market { get; set; }
        public Floor? Floor { get; set; }

        public ICollection<ShopFranchiseShare> FranchiseShares { get; set; } = new List<ShopFranchiseShare>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    }
}
