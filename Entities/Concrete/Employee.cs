using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Employee: IEntity
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Interest { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Taxes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [Range(1, 31)]
        public int PayDay { get; set; }

    }
}
