using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    Tenant(Kiracı)
    //Sözleşmeler bu varlığa bağlanacak(her dükkânda tek aktif kiracı kuralını Sözleşmeyle yöneteceğiz).
    //Temel iletişim ve durum alanları içerir.
    public class Tenant : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Kiracı adı / ticari unvan</summary>
        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>Vergi numarası veya kimlik no (opsiyonel)</summary>
        [MaxLength(50)]
        public string? TaxOrIdNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation (ileride: Contracts)
        public Contract? Contract { get; set; }

    }
}
