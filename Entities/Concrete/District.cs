using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class District : IEntity
    {
        public int Id { get; set; }

        public int ProvinceId { get; set; }   // FK
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ProvinceId))]
        public Province? Province { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
