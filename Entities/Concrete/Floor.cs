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
    public class Floor : IEntity
    {
        public Guid Id { get; set; }

        // ilişki
        [Required]
        public Guid MarketId { get; set; }

        // alanlar
        public string FloorName { get; set; } = null!; // örn: "Zemin", "1. Kat"
        public int Level { get; set; }                 // örn: 0,1,2,-1
        public bool Status { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(MarketId))]
        public Market Market { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Shop> Shops { get; set; } = new List<Shop>();
    }
}
