using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{

    public class FranchiseOwner : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        public string FatherName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; }

        public Gender Gender { get; set; } = Gender.Male;


        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;


        // Navigation

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FranchiseOwnerBalance> FranchiseOwnerBalances { get; set; } = new List<FranchiseOwnerBalance>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ShopFranchiseShare> ShopFranchiseShares { get; set; } = new List<ShopFranchiseShare>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FranchiseBalanceEntry> FranchiseBalanceEntries { get; set; } = new List<FranchiseBalanceEntry>();
    }
}
