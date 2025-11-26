using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class FranchiseOwnerBalance: IEntity
    {
        /// <summary>PK</summary>
        public Guid Id { get; set; }

        /// <summary>İlgili imtiyaz sahibi (FK)</summary>
        public Guid FranchiseOwnerId { get; set; }
        public Guid UpdatedAtBy { get; set; }

        /// <summary>
        /// Güncel bakiye. Pozitif: alacak (biz ona borçluyuz) / Negatif: borç (ona fazla ödeme yapılmış).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Toplam giriş (IN) ve çıkış (OUT) tutarlarını hızlı raporlama için isteğe bağlı saklıyoruz.
        /// (Tutarlılık: Balance ≈ TotalIn - TotalOut. Yuvarlama farkları olabilir.)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIn { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOut { get; set; }

        /// <summary>En son hesap güncelleme zamanı (UTC)</summary>
        public DateTime UpdatedAtUtc { get; set; }
        

        /// <summary>Navigation</summary>
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FranchiseOwnerId))]
        public FranchiseOwner FranchiseOwner { get; set; } = null!;

    }
}
