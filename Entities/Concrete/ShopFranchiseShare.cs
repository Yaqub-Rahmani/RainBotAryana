using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{
    //ShopFranchiseShare(Dükkan–İmtiyaz Pay Oranı, Tarihçeli)
    //Bir dükkândaki birden fazla imtiyaz sahibi ve zamanla değişen oranlar için tarih aralıklı kayıt.
    //Oran yüzdesini decimal(5,2) gibi saklamak için C#’ta decimal kullanıyoruz (EF tarafında HasColumnType("decimal(5,2)") verilebilir).
    public class ShopFranchiseShare : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Bağlı olduğu dükkân</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>Bağlı olduğu imtiyaz sahibi</summary>
        [Required]
        public Guid FranchiseOwnerId { get; set; }

        /// <summary>Pay yüzdesi (ör. 33.33)</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal SharePercent { get; set; }

        /// <summary>Geçerlilik başlangıcı (tarihçeli izleme)</summary>
        [Required]
        public DateOnly EffectiveFrom { get; set; }

        /// <summary>Geçerlilik bitişi (açık uçluysa null)</summary>
        public DateOnly? EffectiveTo { get; set; }

        /// <summary>Opsiyonel açıklama / karar notu</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public Shop? Shop { get; set; }
        public FranchiseOwner? FranchiseOwner { get; set; }

    }
}
