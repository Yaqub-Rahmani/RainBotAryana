using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    // ShopFranchiseShare (Dükkan–İmtiyaz Pay Oranı, Tarihçeli)
    // Bir dükkândaki birden fazla imtiyaz sahibi ve zamanla değişen oranlar için tarih aralıklı kayıt.
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

        // Navigation Properties
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FranchiseOwnerId))]
        public FranchiseOwner FranchiseOwner { get; set; } = null!;
    }
}
