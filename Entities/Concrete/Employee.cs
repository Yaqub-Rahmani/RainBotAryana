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
    public class Employee : IEntity
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

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
