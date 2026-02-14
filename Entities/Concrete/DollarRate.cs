using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class DollarRate : IEntity
    {
        public int Id { get; set; }

        public DateTime EffectiveFromUtc { get; set; }  
        [Column(TypeName = "decimal(18,6)")]
        public decimal AfnPerUsd { get; set; }         

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
