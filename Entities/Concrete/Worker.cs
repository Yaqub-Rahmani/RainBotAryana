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
    public class Worker
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        [Required, MaxLength(200)]
        public string FatherName { get; set; } = default!;

        public Gender Gender { get; set; } = Gender.Male;

        [MaxLength(20)]
        public string NationalId { get; set; }
        public string Duty { get; set; }

        [Required]
        public Guid ShopId { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }

    }
}
