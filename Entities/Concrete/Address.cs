using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{

    public class Address : IEntity
    {
        public Guid Id { get; set; }

        public MediaOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; } 

        // İl/İlçe
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }

        // Detay
        public string? Neighborhood { get; set; }  // Mahalle
        public string? Street { get; set; }        // Cadde/Sokak
        public string? BuildingNo { get; set; }    // Bina/No
        public string? UnitNo { get; set; }        // Daire
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }

        public string Line1 { get; set; } = null!;

        public bool IsPrimary { get; set; } = true;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ProvinceId))]
        public Province? Province { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(DistrictId))]
        public District? District { get; set; }
    }
}
