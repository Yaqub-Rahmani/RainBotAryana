using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public class Tenant : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        [Required, MaxLength(200)]
        public string FatherName { get; set; } = default!;
        public Gender Gender { get; set; } = Gender.Male;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; }

        [MaxLength(50)]
        public string? TaxOrIdNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Accrual> Accruals { get; set; } = new List<Accrual>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CollectionAllocation> Allocations { get; set; } = new List<CollectionAllocation>();
    }
}
