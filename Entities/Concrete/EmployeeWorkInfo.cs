using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    /// <summary>
    /// Employee extra/work info (job + salary settings). One row per employee.
    /// Created/updated after the employee is registered via the general person registration flow.
    /// </summary>
    public class EmployeeWorkInfo : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [MaxLength(200)]
        public string? JobTitle { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryAmount { get; set; }

        /// <summary>
        /// 1=AFN, 2=USD (keep aligned with MoneyCurrency / frontend enums).
        /// </summary>
        public int SalaryCurrency { get; set; } = 1;

        [Range(1, 31)]
        public int PayDay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Interest { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Taxes { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }
    }

}
