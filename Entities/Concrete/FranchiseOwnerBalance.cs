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

        public Guid Id { get; set; }

        public Guid FranchiseOwnerId { get; set; }
        public Guid UpdatedAtBy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIn { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOut { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FranchiseOwnerId))]
        public FranchiseOwner FranchiseOwner { get; set; } = null!;

    }
}
