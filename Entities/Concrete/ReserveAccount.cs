using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public enum ReserveAccountType
    {
        ElectricityReserve = 1,
        ElectricityMain= 2,
        MarketReserve=3,
        MarketMain=4,
        OwnerReserve=5
    }

    public class ReserveAccount : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public ReserveAccountType Type { get; set; }

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();
    }
}
