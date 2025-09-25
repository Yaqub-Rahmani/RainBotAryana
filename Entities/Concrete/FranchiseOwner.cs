using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    FranchiseOwner(İmtiyaz Sahibi)
    //Bir veya birden fazla dükkânda payı olabilen imtiyaz sahibini temsil eder.
    //Kendi bakiye kayıtlarına ileride bağlayacağız; şimdilik kimlik ve iletişim odaklı.
    public class FranchiseOwner : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Ad Soyad veya tüzel unvan</summary>
        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        /// <summary>İletişim için telefon (opsiyonel)</summary>
        [MaxLength(50)]
        public string? Phone { get; set; }

        /// <summary>E-posta (opsiyonel)</summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>Ek notlar (opsiyonel)</summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ShopFranchiseShare> ShopShares { get; set; } = new List<ShopFranchiseShare>();
    }
}
