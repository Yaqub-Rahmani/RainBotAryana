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

        [Required]
        public Guid MarketId { get; set; }

        public string FloorName { get; set; } = null!; 
        public int Level { get; set; }
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
